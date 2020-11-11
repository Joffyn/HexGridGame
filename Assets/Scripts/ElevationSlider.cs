using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElevationSlider : MonoBehaviour
{
    float elevationSliderValue;
    bool activateSliderUpdate;

       void Start()
       {
        gameObject.SetActive(false);
       }

    void Update()
    {
        if(activateSliderUpdate)
        {
            ElevationSliderValue = gameObject.GetComponentInChildren<Slider>().value;
        }
    }

    public float ElevationSliderValue
    {
        get
        {
            return elevationSliderValue;
        }
        set
        {
            elevationSliderValue = value;
        }
    }

    public void sliderUpdateActivate()
        {
        activateSliderUpdate = true;
        }

    public void sliderUpdateDeActivate()
    {
        activateSliderUpdate = false;
    }
}
