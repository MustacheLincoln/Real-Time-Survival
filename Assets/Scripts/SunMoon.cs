using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunMoon : MonoBehaviour
{
    Light sunMoon;
    float angle;
    bool day;
    bool rise;
    Color sunColor = new Color(.9922f, .9843f, .8275f);
    Color moonColor = new Color(.31f, .412f, .533f);
    float totalSeconds;

    private void Start()
    {
        sunMoon = GetComponent<Light>();
    }
    private void Update()
    {
        DateTime timeNow = DateTime.Now;
        DateTime midnight = timeNow.Date;
        TimeSpan dayElapsed = timeNow - midnight;
        totalSeconds = (float)dayElapsed.TotalSeconds;
        angle = totalSeconds / 480;
        day = (totalSeconds > 21600 && totalSeconds <= 64800);
        rise = angle <= 90;
        if (rise)
            sunMoon.intensity = angle / 90;
        else
            sunMoon.intensity = (180 - angle) / 90;
        if (day)
            sunMoon.color = sunColor;
        else
        {
            sunMoon.color = moonColor;
            sunMoon.intensity *= .5f;
        }
        if (angle >= 180)
            angle = angle - 180;
        transform.rotation = Quaternion.Euler(angle, -90, 0);
    }
}
