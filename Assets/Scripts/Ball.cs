using System;
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
            transform.localPosition = get_ball_init_pos();
            transform.rotation = ball_init_rot;
            did_reset = true;
        }

        if (state == "try_game" && is_done)
        {
            transform.localPosition = get_ball_init_pos();
            transform.rotation = ball_init_rot;
            is_done = false;
            episode_started=DateTime.Now;
        }
        // set y axis to zero
        if (transform.localPosition.y >= 0.027f && !is_done)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 0.027f, transform.localPosition.z);
        }

        if (state == "step")
        {
            did_reset = false;
        }

        if (freeze_game)
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