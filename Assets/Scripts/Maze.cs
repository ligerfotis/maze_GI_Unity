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
            case "training":
            {
                return;
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

    int get_x_input(bool discrete)
    {
        if (!discrete)
        {
            var i_x = Input.GetAxis("Horizontal");
            if (i_x > 0)
                input_x = 1;
            else if (i_x < 0)
                input_x = -1;
            else
                input_x = 0;
        }
        else
            input_x = (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0) +
                      (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0);

        return input_x;
    }

    int get_z_input(bool discrete)
    {
        if (!discrete)
        {
            var i_z = Input.GetAxis("Vertical");
            if (i_z > 0)
                input_z = 1;
            else if (i_z < 0)
                input_z = -1;
            else
                input_z = 0;
        }
        else
            input_z = (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ? 1 : 0) +
                      (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ? -1 : 0);

        return input_z;
    }

    void move_maze(bool player_x_axis = false, bool player_z_axis = false, bool discrete = false,
        bool agent_z_axes = false, bool random_human_x_axes = false)
    {
        var local_rotation = transform.eulerAngles;
        local_rotation.y = 0;
        // player move maze

        if (discrete) // not discrete
        {
            if (player_x_axis)
                local_rotation.x =
                    Mathf.Clamp(check_angle(local_rotation.x + get_x_input(true) * game_config.discrete_angle_change),
                        LOWER_BOUND, UPPER_BOUND);

            if (player_z_axis)
                local_rotation.z =
                    Mathf.Clamp(check_angle(local_rotation.z + get_z_input(true) * game_config.discrete_angle_change),
                        LOWER_BOUND, UPPER_BOUND);
        }
        else // not discrete
        {
            if (player_x_axis)
                local_rotation.x =
                    Mathf.Clamp(
                        check_angle(local_rotation.x + get_x_input(false) * game_config.human_speed * Time.deltaTime),
                        LOWER_BOUND, UPPER_BOUND);

            if (player_z_axis)
                local_rotation.z =
                    Mathf.Clamp(
                        check_angle(local_rotation.z + get_z_input(false) * game_config.human_speed * Time.deltaTime),
                        LOWER_BOUND, UPPER_BOUND);
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
            input_x = x_random;
            local_rotation.x =
                Mathf.Clamp(check_angle(local_rotation.x + x_random * game_config.human_speed * Time.deltaTime),
                    LOWER_BOUND, UPPER_BOUND);
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