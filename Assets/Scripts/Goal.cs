using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Constants;
using static GameStates;
using static Helpers;

public class Goal : MonoBehaviour
{
    public Button START_EXPERIMENT;
    public GameObject WIN_TEXT;
    public GameObject WIN;
    public GameObject BALL;
    public AudioSource WIN_AUDIO;
    DateTime start_time;

    // Start is called before the first frame update
    void Start()
    {
        WIN.SetActive(false);
        start_time = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name != "Ball") return;

        var seconds = (DateTime.Now - start_time).TotalSeconds;
        start_time = DateTime.Now;
        WIN_TEXT.GetComponent<Text>().text = $"{(int) seconds} sec";
        WIN.SetActive(true);
        WIN_AUDIO.Play();
        StartCoroutine(nameof(remove_win));
        START_EXPERIMENT.interactable = true;
        if (state != "try_game")
        {
            on_freeze = true;
            is_done = true;
        }

        print("GOAL");
    }

    IEnumerator remove_win()
    {
        // suspend execution for 5 seconds
        yield return new WaitForSeconds(1);
        WIN.SetActive(false);
    }
}