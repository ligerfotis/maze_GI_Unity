using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;
using static Helpers;
using static GameStates;

public class Finished : MonoBehaviour
{
    public GameObject FinishedUI;

    // Start is called before the first frame update
    void Start()
    {
        FinishedUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        FinishedUI.SetActive(state == "finished");
    }
}