using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static Constants;
using static GameStates;
using static Helpers;

public class GameManager : MonoBehaviour
{
    public GameObject SETTINGS_PANEL;
    public GameObject INIT_UI;
    public GameObject PAUSE_BTN;
    public GameObject MAIN_MENU_UI;
    public GameObject MODE_UI;
    public GameObject FINISHED_UI;
    public Text MODE_TEXT;
    public int TIME_SCALE = 1;
    DateTime pause_timestamp;
 
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        // OnDemandRendering.renderFrameInterval = 60;
        freeze_game = true;

        Time.timeScale = TIME_SCALE;
    }

    // Start is called before the first frame update
    void Start()
    {
        MAIN_MENU_UI.SetActive(false);
        show_settings(false);
    }

    // Update is called once per frame
    void Update()
    {
        PAUSE_BTN.SetActive(on_pause);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            on_pause = !on_pause;
            if (on_pause)
            {
                pause_time = 0;
                pause_timestamp = DateTime.Now;
            }
            else
            {
                pause_time = (float) (DateTime.Now - pause_timestamp).TotalSeconds;
            }
        }

        if (on_pause)
        {
            Time.timeScale = 0;
            return;
        }

        Time.timeScale = TIME_SCALE;
        PAUSE_BTN.SetActive(on_pause);


        switch (step_request.mode)
        {
            case "init":
            {
                MODE_UI.SetActive(false);
                break;
            }
            case "train":
            {
                MODE_UI.SetActive(true);
                MODE_TEXT.text = "Train";
                break;
            }
            case "test":
            {
                MODE_UI.SetActive(true);
                MODE_TEXT.text = "Test";
                break;
            }
            default:
            {
                MODE_UI.SetActive(false);
                break;
            }
        }
    }

    public void set_quality(int quality)
    {
        QualitySettings.SetQualityLevel(quality);
        show_settings(false);
    }

    public void show_settings(bool show) => SETTINGS_PANEL.SetActive(show);

    public void start_experiment()
    {
        set_state("start");
        INIT_UI.SetActive(false);
        MAIN_MENU_UI.SetActive(false);
    }

    public void try_game()
    {
        MAIN_MENU_UI.SetActive(true);
        set_state("try_game");
        freeze_game = false;
        INIT_UI.SetActive(false);
    }

    public void return_to_menu()
    {
        MAIN_MENU_UI.SetActive(false);
        set_state("init");
        freeze_game = true;
        INIT_UI.SetActive(true);
    }
}