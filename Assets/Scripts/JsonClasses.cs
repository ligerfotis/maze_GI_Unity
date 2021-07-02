using System;

[Serializable]
public class ResetResponse
{
    public float[] observation;
    public int setting_up_duration = 0;
    public string command = "reset";
}


[Serializable]
public class StepResponse
{
    public float[] observation;
    public bool done;
    public int fps;
    public int duration_pause;
    public float distance_from_goal;
    public string command = "step";
}

[Serializable]
public class StepRequest
{
    public int action_agent = 0;
    public int action_duration = 0;
    public bool timed_out = false;
    public string mode = "init";
    public string command = "step";
}

[Serializable]
public class TrainingRequest
{
    public int cycle = 0;
    public int total_cycles = 0;
    public string command = "training";
}

[Serializable]
public class CommandRequest
{
    public string command;
    public StepRequest step_request = null;
    public TrainingRequest training_request = null;
}

[Serializable]
public class GameConfig
{
    public bool discrete_input = false;
    public int max_duration = 40;
    public bool human_assist = false;
    public float action_duration = 0.2f;
    public float human_speed = 0.2f;
    public float agent_speed = 0.2f;
    public float discrete_angle_change = 10;
    public int start_up_screen_display_duration = 2;
    public int timeout_screen_display_duration = 3;
    public int goal_screen_display_duration = 3;
}