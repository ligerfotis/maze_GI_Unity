using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class Constants
{


    public const int PORT = 6610;
    public const int UPPER_BOUND = 30;
    public const int LOWER_BOUND = -30;
    public static Quaternion ball_init_rot = new Quaternion(0f, 0f, 0f, 0f);

    public const int MAX_NO_DATA = 5000;

    static List<Vector3> ball_init_pos = new List<Vector3>()
    {
        new Vector3(-0.19f, 0.025f, 0.19f), //upper right corner
        new Vector3(0.1f, 0.025f, 0.19f), //bottom right corner
        new Vector3(-0.19f, 0.025f, -0.1f) //upper left corner
    };

    public static Vector3 get_ball_init_pos()
    {
        // return ball_init_pos[0];

        return ball_init_pos[Random.Range(0, ball_init_pos.Count)];
    }
}