using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameStates
{
    // public static string HOST = "http://panos-server.duckdns.org:8080";
    // public static string HOST = "http://2.85.241.49:8080";      // maze-server
    public static string HOST = "http://maze3d.duckdns.org:8080"; // maze-server
    // public static string HO1ST = "https://maze-server.app.orbitsystems.gr";
    // public static string HOST = "http://localhost:8080";
    public static GameConfig game_config;
    public static bool freeze_game = true;
    public static bool is_done = false;
    public static bool on_pause = false;
    public static float pause_time = 0f;
    public static DateTime episode_started;
    public static float episode_paused_time = 0;

    public static StepRequest step_request;
    public static StepResponse step_response;
    public static ResetResponse reset_response;
    public static TrainingRequest training_request;
    public static CommandRequest command_request;


    public static string state = "init";
    public static string prev_state = "init";

    public static int input_x = 0;
    public static int input_z = 0;

    public static float x_angular_speed = 0.0f;
    public static float z_angular_speed = 0.0f;

    public static void set_state(string s)
    {
        if (state != s)
            prev_state = state;
        state = s;
    }

    public static void revert_to_prev_state() => state = prev_state;
}