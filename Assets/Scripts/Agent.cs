using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static Constants;
using static Helpers;
using Random = UnityEngine.Random;
using static GameStates;

public class Agent : MonoBehaviour
{
    public GameObject BALL;
    public GameObject GOAL;
    Rigidbody r_ball;

    void Awake()
    {
        StartCoroutine(get_config());
    }

    void Start()
    {
        r_ball = BALL.gameObject.GetComponent<Rigidbody>();
        step_request = new StepRequest();
        training_request = new TrainingRequest();
        step_response = new StepResponse();
        set_step_response();
        reset_response = new ResetResponse {observation = get_observation()};
        StartCoroutine(network_manager());
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

    IEnumerator do_command_request(string method, string route, System.Action callback)
    {
        UnityWebRequest res = UnityWebRequest.Get(HOST + "/player_ready");
        yield return res.SendWebRequest();
        if (!is_request_success(res))
        {
            set_state("start");
            yield return new WaitForSeconds(.01f);
        }

        command_request = JsonUtility.FromJson<CommandRequest>(res.downloadHandler.text);
        callback();
    }

    IEnumerator network_manager()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.05f);
            UnityWebRequest res;
            switch (state)
            {
                case "training":
                case "start":
                {
                    on_freeze = true;
                    yield return do_command_request("GET", "/player_ready", () =>
                    {
                        set_state(command_request.command);
                        if (state == "step")
                            step_request = command_request.step_request;
                        if (state == "training")
                        {
                            training_request = command_request.training_request;
                        }
                    });
                    break;
                }
                case "reset":
                {
                    reset_response = new ResetResponse {observation = get_observation()};
                    var post_data = JsonUtility.ToJson(reset_response);
                    res = UnityWebRequest.Post(HOST + "/reset_done", UnityWebRequest.kHttpVerbPOST);
                    res.SetRequestHeader("Content-Type", "application/json");
                    var json_bytes = Encoding.UTF8.GetBytes(post_data);
                    res.uploadHandler = new UploadHandlerRaw(json_bytes);

                    yield return res.SendWebRequest();
                    if (!is_request_success(res))
                    {
                        set_state("start");
                        yield return new WaitForSeconds(1);
                        continue;
                    }

                    command_request = JsonUtility.FromJson<CommandRequest>(res.downloadHandler.text);
                    set_state(command_request.command);

                    on_freeze = false;
                    is_done = false;
                    reset_pos = true;
                    break;
                }

                case "step":
                {
                    on_freeze = false;

                    var post_data = JsonUtility.ToJson(step_response);
                    res = UnityWebRequest.Post(HOST + "/observation", UnityWebRequest.kHttpVerbPOST);
                    res.SetRequestHeader("Content-Type", "application/json");
                    var json_bytes = Encoding.UTF8.GetBytes(post_data);
                    res.uploadHandler = new UploadHandlerRaw(json_bytes);

                    yield return res.SendWebRequest();
                    if (!is_request_success(res))
                    {
                        set_state("start");

                        yield return new WaitForSeconds(1);
                        continue;
                    }

                    command_request = JsonUtility.FromJson<CommandRequest>(res.downloadHandler.text);
                    if (state == "step")
                    {
                        step_request = command_request.step_request;
                        set_state(command_request.command);
                        yield return new WaitForSeconds(game_config.action_duration - 0.05f);
                    }

                    if (state == "training")
                        training_request = command_request.training_request;

                    set_state(command_request.command);
                    set_step_response();
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
        step_response.fps = 60;
        step_response.duration_pause = 0;
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