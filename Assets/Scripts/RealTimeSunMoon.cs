using System;
using UnityEngine;

public class RealTimeSunMoon : MonoBehaviour
{
    Light directionalLight;
    Color sunColor = new Color(.9922f, .9843f, .8275f);
    Color moonColor = new Color(.31f, .412f, .533f);
    float fadeAngle = 20;
    float dayIntensity = 1;
    float nightIntensity = .5f;

    private void Start() { directionalLight = GetComponent<Light>(); }

    private void Update()
    {
        TimeSpan timeSinceDawn = DateTime.Now - DateTime.Now.Date.AddHours(6);
        float secondsSinceDawn = (float)timeSinceDawn.TotalSeconds;
        float angle = secondsSinceDawn / 240;
        if (angle < 180)
        {
            directionalLight.color = sunColor;
            directionalLight.intensity = dayIntensity;
            if (angle < fadeAngle)
                directionalLight.intensity = (angle / fadeAngle) * dayIntensity;
            if (angle >= 180 - fadeAngle)
                directionalLight.intensity = ((180 - angle) / fadeAngle) * dayIntensity;
        }
        else
        {
            angle = angle - 180;
            directionalLight.color = moonColor;
            directionalLight.intensity = nightIntensity;
            if (angle < fadeAngle)
                directionalLight.intensity = (angle / fadeAngle) * nightIntensity;
            if (angle >= 180 - fadeAngle)
                directionalLight.intensity = ((180 - angle) / fadeAngle) * nightIntensity;
        }
        transform.rotation = Quaternion.Euler(angle, -90, 0);
    }
}
