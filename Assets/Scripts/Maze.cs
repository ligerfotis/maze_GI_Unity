using System.Collections;
using UnityEngine;
using static Constants;
using static GameStates;
using static Helpers;
using Random = UnityEngine.Random;

public class Maze : MonoBehaviour
{
    public bool RANDOM_HUMAN = false;
    int x_random = 0;

    void Start()
    {
        StartCoroutine(random_x_action());
    }

    void Update()
    {
        if (on_pause)
            return;
        switch (state)
        {
            case "init":
                break;
            case "try_game":
            {
                move_maze(true, true, game_config.discrete_input);
                break;
            }
            case "reset":
            {
                reset_maze();
                break;
            }
            case "step":
            {
                move_maze(true, game_config.human_assist, game_config.discrete_input, true, RANDOM_HUMAN);
                break;
            }
            default:
            {
                move_maze(true, game_config.human_assist, game_config.discrete_input);
                break;
            }
        }
    }

    void reset_maze()
    {
        var local_rotation = transform.eulerAngles;
        local_rotation.x = 0;
        local_rotation.y = 0;
        local_rotation.z = 0;
        transform.eulerAngles = local_rotation;
    }


    void move_maze(bool player_x_axis = false, bool player_z_axis = false, bool discrete = false,
        bool agent_z_axes = false, bool random_human_x_axes = false)
    {
        var local_rotation = transform.eulerAngles;
        local_rotation.y = 0;
        // player move maze
        float input_x;
        float input_z;
        if (discrete) // not discrete
        {
            input_x = (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0) +
                      (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0);
            input_z = (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ? 1 : 0) +
                      (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ? -1 : 0);
            local_rotation.y = 0;

            if (player_x_axis)
                local_rotation.x =
                    Mathf.Clamp(check_angle(local_rotation.x + input_x * game_config.discrete_angle_change),
                        LOWER_BOUND, UPPER_BOUND);
            if (player_z_axis)
                local_rotation.z =
                    Mathf.Clamp(check_angle(local_rotation.z + input_z * game_config.discrete_angle_change),
                        LOWER_BOUND, UPPER_BOUND);
        }
        else // not discrete
        {
            if (player_x_axis)
            {
                input_x = Input.GetAxis("Horizontal");
                var x = 0f;
                print(game_config.human_speed);
                if (input_x != 0)
                {
                    x = game_config.human_speed * Time.deltaTime;
                    x = input_x > 0 ? x : -x;
                }

                local_rotation.x = Mathf.Clamp(check_angle(local_rotation.x + x), LOWER_BOUND, UPPER_BOUND);
            }

            if (player_z_axis)
            {
                input_z = Input.GetAxis("Vertical");
                var z = 0f;
                if (input_z != 0)
                {
                    z = game_config.human_speed * Time.deltaTime;
                    z = input_z > 0 ? z : -z;
                }

                local_rotation.z = Mathf.Clamp(check_angle(local_rotation.z + z), LOWER_BOUND, UPPER_BOUND);
            }
        }

        // agent move maze
        if (agent_z_axes)
        {
            print("agent_z_axes");
            var z = -step_request.action_agent * game_config.agent_speed * Time.deltaTime;
            local_rotation.z = Mathf.Clamp(check_angle(local_rotation.z + z), LOWER_BOUND, UPPER_BOUND);
        }

        // random_human move maze
        if (random_human_x_axes)
        {
            var x = x_random * game_config.human_speed * Time.deltaTime;
            local_rotation.x = Mathf.Clamp(check_angle(local_rotation.x + x), LOWER_BOUND, UPPER_BOUND);
        }

        transform.eulerAngles = local_rotation;
    }

    IEnumerator random_x_action()
    {
        while (true)
        {
            x_random = Random.Range(-1, 2);
            yield return new WaitForSeconds(0.2f);
        }
    }
}