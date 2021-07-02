using System.Collections.Generic;
using UnityEngine;

public static class GameStates
{
    public static GameConfig game_config;
    public static bool on_freeze = true;
    public static bool is_done = false;
    public static bool on_pause = false;
    public static float pause_time = 0f;

    public static StepRequest step_request;
    public static StepResponse step_response;
    public static ResetResponse reset_response;
    public static TrainingRequest training_request;
    public static CommandRequest command_request;


    public static string state = "init";
    public static string prev_state = "init";

    public static int input_x = 0;
    public static int input_z = 0;

    public static void set_state(string s)
    {
        if (state != s)
            prev_state = state;
        state = s;
    }

    public static void revert_to_prev_state() => state = prev_state;
}