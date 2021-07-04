using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Constants;
using static GameStates;
using static Helpers;

public class Countdown : MonoBehaviour
{
    public GameObject OUTER;
    public Image PROGRESS;
    public Text COUNTDOWN_TEXT;

    // Start is called before the first frame update
    void Start()
    {
        OUTER.SetActive(false);
        episode_started = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == "step" && !on_pause)
        {
            OUTER.SetActive(true);
            var elapsed_time = (float) (DateTime.Now - episode_started).TotalSeconds - episode_paused_time - 1;
            elapsed_time = Math.Min(elapsed_time, game_config.max_duration);
            var percent = elapsed_time / game_config.max_duration;
            PROGRESS.fillAmount = elapsed_time / game_config.max_duration;
            PROGRESS.color = percent > 0.75f ? new Color(255, 0, 0) : new Color(0, 255, 0);

            COUNTDOWN_TEXT.text = ((int) (game_config.max_duration - elapsed_time)).ToString();
        }
        else
            OUTER.SetActive(false);
    }
}