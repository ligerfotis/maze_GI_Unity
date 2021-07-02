using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Constants;
using static Helpers;
using Random = UnityEngine.Random;
using static GameStates;
using Object = UnityEngine.Object;

public class Agent : MonoBehaviour
{
    public GameObject BALL;
    public GameObject GOAL;
    Rigidbody r_ball;
    int fps_counter = 1;
    int fps_adder = 60;

    void Awake()
    {
        StartCoroutine(get_config());
    }

    void Start()
    {
        print(HOST);

        r_ball = BALL.gameObject.GetComponent<Rigidbody>();
        step_request = new StepRequest();
        training_request = new TrainingRequest();
        step_response = new StepResponse();
        reset_response = new ResetResponse();
        StartCoroutine(network_manager());
    }

    void Update()
    {
        fps_adder += (int) (1f / Time.unscaledDeltaTime);
        fps_counter++;
    }

    IEnumerator get_config()
    {
        while (true)
        {
            var res = UnityWebRequest.Get(HOST + "/config");
            yield return res.SendWebRequest();
            if (!is_request_success(res))
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            game_config = JsonUtility.FromJson<GameConfig>(res.downloadHandler.text);
            print("Setting Config");
            break;
        }
    }

    IEnumerator do_command_request(string method, string route, string json_data = null, System.Action callback = null)
    {
        UnityWebRequest res;
        if (method == "GET")
            res = UnityWebRequest.Get(HOST + "/player_ready");
        else
        {
            res = UnityWebRequest.Post(HOST + "/reset_done", UnityWebRequest.kHttpVerbPOST);
            res.SetRequestHeader("Content-Type", "application/json");
            var json_bytes = Encoding.UTF8.GetBytes(json_data);
            res.uploadHandler = new UploadHandlerRaw(json_bytes);
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
    {
        while (true)
        {
            print(state);
            yield return new WaitForSeconds(0.05f);
            switch (state)
            {
                case "start":
                {
                    on_freeze = true;
                    yield return do_command_request("GET", "/player_ready");
                    break;
                }
                case "reset":
                {
                    reset_response = new ResetResponse {observation = get_observation()};
                    yield return do_command_request("POST", "/player_ready", reset_response.to_json());
                    on_freeze = true;
                    is_done = false;
                    break;
                }
                case "step":
                {
                    on_freeze = false;
                    step_request = command_request.step_request;
                    yield return new WaitForSeconds(game_config.action_duration - 0.05f);
                    set_step_response();
                    yield return do_command_request("POST", "/observation", step_response.to_json());
                    break;
                }
                case "training":
                {
                    training_request = command_request.training_request;
                    yield return do_command_request("GET", "/player_ready");
                    break;
                }
                case "finished":
                {
                    break;
                }
            }
        }
    }


    void set_step_response()
    {
        step_response.observation = get_observation();
        step_response.distance_from_goal =
            Vector3.Distance(GOAL.transform.localPosition, BALL.transform.localPosition);
        step_response.done = is_done ? is_done : step_request.timed_out;

        step_response.fps = fps_adder / fps_counter;
        fps_counter = 1;
        fps_adder = 60;

        step_response.duration_pause = 0; //TODO 
        step_response.human_action = input_x;
        step_response.agent_action = input_z;
    }

    float[] get_observation()
    {
        var input_x = Input.GetAxis("Horizontal");
        var x_speed = 0f;
        if (input_x > 0)
            x_speed = game_config.human_speed;
        else if (input_x < 0)
            x_speed = -game_config.human_speed;

        var position = BALL.transform.position;
        var velocity = r_ball.velocity;

        var local_rotation = transform.eulerAngles;

        return new[]
        {
            position.z, -position.x,
            velocity.z, -velocity.x,
            check_angle(local_rotation.x), check_angle(local_rotation.z),
            x_speed, step_request.action_agent * game_config.agent_speed
        };
    }
}