using System.Collections;
using UnityEngine;
using static Constants;
using static GameStates;
using static Helpers;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
public class Maze : MonoBehaviour
{
    public bool RANDOM_HUMAN = false;
    int x_random = 0;
	public List<float[]> observations = new List<float[]>();
    
	public GameObject BALL;

    Rigidbody r_ball;


    void Start()
    {
		// todo: start with zero action
        StartCoroutine(random_x_action());
        r_ball = BALL.gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (on_pause)
            return;
        switch (state)
        {
            case "init":
				is_done = true;
                break;
            case "try_game":
            {
                move_maze(true, true, game_config.discrete_input);
                if (is_done)
                    reset_maze();
				// todo: if timeout also reset
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
            case "goal_reached":
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
        //print(string.Format("x_angular_speed: {0}", x_angular_speed));
        //print(string.Format("z_angular_speed: {0}", z_angular_speed));

        if (player_x_axis)
        {
            x_angular_speed = (discrete) ? get_x_input(true) * game_config.discrete_angle_change : get_x_input(false) * game_config.human_speed * Time.deltaTime;
            local_rotation.x =
                Mathf.Clamp(check_angle(local_rotation.x + x_angular_speed),
                    LOWER_BOUND, UPPER_BOUND);
        }

        if (player_z_axis)
        {
            z_angular_speed = (discrete) ? get_z_input(true) * game_config.discrete_angle_change : get_z_input(false) * game_config.human_speed * Time.deltaTime;
            local_rotation.z =
                Mathf.Clamp(check_angle(local_rotation.z + z_angular_speed),
                    LOWER_BOUND, UPPER_BOUND);

        }
            
        // agent move maze
        if (agent_z_axes)
        {
            z_angular_speed = step_request.action_agent * game_config.agent_speed * Time.deltaTime;
            local_rotation.z = Mathf.Clamp(check_angle(local_rotation.z + z_angular_speed), LOWER_BOUND, UPPER_BOUND);
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
        //print("observation");
		// float[] obs = get_observation(x_angular_speed, z_angular_speed);
		// observations.Add(obs);
		// if (observations.Count == 500)
		// {
  //       	StreamWriter writer = new StreamWriter(Application.dataPath + "/Data/"  + "right_1strike.csv");
  //
  //       	for (int i = 0; i < observations.Count; ++i)
  //       	{
  //           	writer.WriteLine(observations[i][0]+","+ observations[i][1]+","+ observations[i][2]+","+ observations[i][3]+","+ observations[i][4]+","+ observations[i][5]+","+ observations[i][6]+","+ observations[i][7]);
  //       	}
		// 	print("saved");
		// }

		//print(string.Format("BallPosX: {0} | BallPosY: {1} | BallVelX: {2} | BallVelY: {3} | TrayAngleX: {4} | TrayAngleY: {5} | TrayAngleVelX: {6} | TrayAngleVelY: {7}", obs[0], obs[1], obs[2], obs[3], obs[4], obs[5], obs[6], obs[7]));
    }

    IEnumerator random_x_action()
    {
        while (true)
        {
            x_random = Random.Range(-1, 2);
            yield return new WaitForSeconds(0.2f);
        }
    }

	float[] get_observation(float x_angular_speed, float z_angular_speed)
    {

        var position = BALL.transform.position;
        var velocity = r_ball.velocity;

        var local_rotation = transform.eulerAngles;
        return new[]
        {
            position.z, -position.x,
            velocity.z, -velocity.x,
            check_angle(local_rotation.x), check_angle(local_rotation.z),
            x_angular_speed, z_angular_speed
        };
    }

}