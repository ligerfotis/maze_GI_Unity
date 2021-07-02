using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Constants;
using static GameStates;

public class ProgressBar : MonoBehaviour
{
    public GameObject TRAINING_UI;
    public Slider TRAINING_SLIDER;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (state == "training" || prev_state == "training")
        {
            TRAINING_UI.gameObject.SetActive(true);
            on_freeze = true;
            TRAINING_SLIDER.minValue = 0;
            TRAINING_SLIDER.maxValue = training_request.total_cycles;
            TRAINING_SLIDER.value = training_request.cycle;
        }
        else
        {
            TRAINING_UI.gameObject.SetActive(false);
        }
    }
}