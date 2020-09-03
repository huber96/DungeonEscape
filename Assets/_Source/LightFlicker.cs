using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    Light mainLight;

    private void Awake()
    {
        mainLight = this.GetComponent<Light>();
    }

    private void Update()
    {
        mainLight.intensity = 1 + 0.075f * Mathf.Sin(Time.timeSinceLevelLoad * 3);
        mainLight.range = 8 + 0.075f * Mathf.Sin(Time.timeSinceLevelLoad * 3);
    }
}
