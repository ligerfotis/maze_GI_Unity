using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;
using static Helpers;
using static GameStates;

public class Ball : MonoBehaviour
{
    Rigidbody r_ball;

    bool did_reset = false;

    // Start is called before the first frame update
    void Start()
    {
        r_ball = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (state == "reset" && !did_reset)
        {
            print("FREEZE RESET");
            transform.localPosition = get_ball_init_pos();
            transform.rotation = ball_init_rot;
            freeze();
            did_reset = true;
        }

        if (state == "step")
        {
            did_reset = false;
        }

        if (on_freeze)
            freeze();
        else
            unfreeze();
    }

    void freeze()
    {
        r_ball.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                             RigidbodyConstraints.FreezePositionY;
    }

    void unfreeze()
    {
        r_ball.constraints = RigidbodyConstraints.None;
    }
}