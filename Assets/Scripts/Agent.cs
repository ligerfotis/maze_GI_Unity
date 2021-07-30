using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using static Constants;
using static Helpers;
using static GameStates;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

public class Agent : MonoBehaviour
{
    public GameObject BALL;
    public GameObject GOAL;
    public GameObject TIMEOUT_UI;
    Rigidbody r_ball;
    int fps_counter = 1;
    int fps_adder = 60;
    float request_duration = 0;

    void Awake()
    {
        StartCoroutine(get_config());   // retrieve the configuration file sent from MazeRL
        TIMEOUT_UI.SetActive(false);        // Disable the timeout UI
    }

    void Start()
    {
        r_ball = BALL.gameObject.GetComponent<Rigidbody>(); // initialize a rigid body for the goal.
        step_request = new StepRequest();           // initialize a step request
        training_request = new TrainingRequest();   // initialize a training request
        step_response = new StepResponse();         // initialize a step response that will be answered back to MazeRL after the request received
        reset_response = new ResetResponse();       // initialize a reset response to use whenever reset is requested by MazeRL
        StartCoroutine(network_manager());    // start the network manager that will handle the above requests that will be received and the responses to be send
    }

    void Update()
    {   // count fps
        fps_adder += (int) (1f / Time.unscaledDeltaTime);
        fps_counter++;
    }

    IEnumerator get_config()
    /*
     * Retrieve the configuration file sent from MazeRL
     */
    {
        // set conenction to the HOST (Maze-Server)
        while (true)
        {
            var res = UnityWebRequest.Get(HOST + "/env_variables");
            yield return res.SendWebRequest();
            if (!is_request_success(res))
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            HOST = JsonUtility.FromJson<EnvVariables>(res.downloadHandler.text).host;
            print("Setting EnvVariables");
            print("HOST: " + HOST);
            break;
        }

        while (true)
        {
            // ask from the host (Maze-Server) to send the configuration file
            var res = UnityWebRequest.Get(HOST + "/config");
            yield return res.SendWebRequest();
            // if request is not successful keep requesting
            if (!is_request_success(res))
            {
                yield return new WaitForSeconds(1f);
                continue;
            }
            // if host returned the configuration file save it to game_config
            game_config = JsonUtility.FromJson<GameConfig>(res.downloadHandler.text);
            print("Setting Config");
            break;
        }
    }

    IEnumerator do_command_request(string method, string route, string json_data = null, Action callback = null)
    /*
     * handle requests and responses
     */
    {
        UnityWebRequest res;
        if (method == "GET")
            res = UnityWebRequest.Get(HOST + route);    // receive request from MazeRL
        else
        {
            res = UnityWebRequest.Post(HOST + route, UnityWebRequest.kHttpVerbPOST);
            res.SetRequestHeader("Content-Type", "application/json");
            var json_bytes = Encoding.UTF8.GetBytes(json_data);
            res.uploadHandler = new UploadHandlerRaw(json_bytes); // send response to MazeRL
        }

        yield return res.SendWebRequest();
        if (!is_request_success(res))
        {
            set_state("start");
            yield return new WaitForSeconds(.01f);
        }
        else
        {
            command_request = JsonUtility.FromJson<CommandRequest>(res.downloadHandler.text);
            set_state(command_request.command);
            callback?.Invoke();
        }
    }

    IEnumerator network_manager()
    /*
     * Manager for the different states of the communication
     */
    {
        while (true)
        {
            yield return new WaitForSeconds(0.005f);
            switch (state)
            {
                case "start":
                {
                    freeze_game = true;
                    yield return do_command_request("GET", "/player_ready");
                    break;
                }
                case "reset":
                {
                    reset_response = new ResetResponse {observation = get_observation()};       // set the response with the initial observation of the environment
                    yield return do_command_request("POST", "/reset_done", reset_response.to_json(), () =>
                    {
                        episode_paused_time = 0;
                        pause_time = 0;
                        episode_started = DateTime.Now;
                    });     // send the response
                    freeze_game = true;   // freeze the game
                    is_done = false;    // game will start
                    break;
                }
                case "step":
                {
                    freeze_game = false;    // unfreeze the game

                    step_request = command_request.step_request;    // retrieve step request
                    // calculate the duration of the action. subtract the time that the request had to travel through
                    // internet, because during this time the action was had been executed
                    // We assume that the deviation of two consecutive request delays is the same.
                    var action_duration = game_config.action_duration - request_duration - 0.005f;  
                    
                    yield return new WaitForSeconds(action_duration < 0 ? 0 : action_duration);     // wait to execute step for "action duration" time
                    set_step_response();
                    var start_request_time = DateTime.Now;
                    yield return do_command_request("POST", "/observation", step_response.to_json(), () =>
                    {
                        request_duration = (float) (DateTime.Now - start_request_time).TotalSeconds;
                        fps_counter = 1;
                        fps_adder = 60;
                        episode_paused_time += pause_time;
                        pause_time = 0;
                        if (step_request.timed_out) TIMEOUT_UI.SetActive(true);
                        if (!step_response.done) return;
                        freeze_game = true;
                        set_state("goal_reached");
                    });     // construct the response to the step request

                    break;
                }
                case "training":
                /*
                 * During training in MazeRL we expect to receive a request during each cycle of the training
                 * to keep the connenction alive and display the progress.
                 */
                {
                    training_request = command_request.training_request;
                    yield return do_command_request("GET", "/player_ready");    // handle the training request
                    break;
                }
                case "finished":
                    /*
                     * the experiment has finished
                     */
                {
                    yield return new WaitForSeconds(5f);
                    set_state("init");
                    SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);  // display final scene
                    break;
                }
                case "goal_reached":
                    /*
                     * Goal has been reached
                     */
                {
                    yield return new WaitForSeconds(game_config.popup_window_time);     // wait for 'popup_window_time' seconds
                    revert_to_prev_state(); // go to previous state
                    TIMEOUT_UI.SetActive(false);    //  deactivate the goal message that had been displaying since goal reached
                    freeze_game = false;    // unfreeze the game
                    break;
                }
            }
        }
    }


    void set_step_response()
    /*
     * Construct the response to MazeRL
     */
    {
        step_response.observation = get_observation();
        step_response.distance_from_goal =
            Vector3.Distance(GOAL.transform.localPosition, BALL.transform.localPosition);
        step_response.done = is_done ? is_done : step_request.timed_out;

        step_response.fps = fps_adder / fps_counter;


        step_response.duration_pause = pause_time;

        step_response.human_action = input_x;
        step_response.agent_action = step_request.action_agent;
    }

    float[] get_observation()
    /*
     * returns the observation of the environment
     * [ball_position_x, ball_position_y, ball_velocity_x, ball_velocity_y, tray_angle_around_y, tray_angle_around_x,
     * tray_angular_velocity_around_y, tray_angular_velocity_around_x]
     */
    {
        var position = BALL.transform.position;
        var velocity = r_ball.velocity;

        var local_rotation = transform.eulerAngles;

        return new[]
        {
            position.z, position.x,
            velocity.z, velocity.x,
            check_angle(local_rotation.z), check_angle(local_rotation.x),
            z_angular_speed, x_angular_speed
        };
    }
}