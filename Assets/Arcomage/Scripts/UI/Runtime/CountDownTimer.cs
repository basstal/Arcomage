using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CountDownTimer : MonoBehaviour
{
    private int TotalSeconds;

    private Coroutine cur;
    private bool Loop;

    public TMPro.TextMeshProUGUI text;

    public void SetCountDown(float seconds, bool loop)
    {
        TotalSeconds = (int)seconds;
        Loop = loop;
        StopCountDown();
        CountDowmStart();
    }

    public void SetCountDown(float hour, float min, float seconds)
    {
        TotalSeconds = (int)((new TimeSpan((int)hour, (int)min, (int)seconds)).TotalSeconds);
        StopCountDown();
        CountDowmStart();
    }

    private void CountDowmStart()
    {
        cur = StartCoroutine("StartCountDowm", Random.Range(0, 10));
    }

    IEnumerator StartCountDowm(int a)
    {
        do
        {
            var seconds = TotalSeconds;
            while (seconds > 0)
            {
                seconds--;
                text.text = SecondsToTimer(seconds);
                yield return new WaitForSeconds(1f);
            }
        } while (Loop);
    }


    private string SecondsToTimer(int totalSeconds)
    {
        TimeSpan time = new TimeSpan(0, 0, totalSeconds);
        return time.ToString(@"hh\:mm\:ss");
    }

    private void OnDisable()
    {
        StopCoroutine(cur);
    }

    public void StopCountDown()
    {
        if (cur != null) StopCoroutine(cur);
    }
}
