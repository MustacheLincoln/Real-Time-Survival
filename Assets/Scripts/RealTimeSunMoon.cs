using System;
using UnityEngine;

public class RealTimeSunMoon : MonoBehaviour
{
    Light directionalLight;
    Color sunColor = new Color(.9922f, .9843f, .8275f);
    Color moonColor = new Color(.31f, .412f, .533f);
    float fadeAngle = 20;
    float nightIntensityModifier = .5f;

    private void Start() { directionalLight = GetComponent<Light>(); }

    private void Update()
    {
        TimeSpan timeSinceDawn = DateTime.Now - DateTime.Now.Date.AddHours(6);
        float secondsSinceDawn = (float)timeSinceDawn.TotalSeconds;
        float angle = secondsSinceDawn / 240;
        if (angle < 180)
        {
            directionalLight.color = sunColor;
            if (angle < fadeAngle)
                directionalLight.intensity = angle / fadeAngle;
            if (angle >= 180 - fadeAngle)
                directionalLight.intensity = (180 - angle) / fadeAngle;
        }
        else
        {
            directionalLight.color = moonColor;
            angle = angle - 180;
            if (angle < fadeAngle)
                directionalLight.intensity = (angle / fadeAngle) * nightIntensityModifier;
            if (angle >= 180 - fadeAngle)
                directionalLight.intensity = ((180 - angle) / fadeAngle) * nightIntensityModifier;
        }
        transform.rotation = Quaternion.Euler(angle, -90, 0);
    }
}
