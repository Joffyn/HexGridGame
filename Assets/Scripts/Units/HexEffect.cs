using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexEffect : MonoBehaviour
{

    void OnEnable()
    {
        if (location)
        {
            transform.localPosition = location.Position;
        }
    }

    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            if (location)
            {
                location.Unit = null;
            }
            location = value;
            value.Effect = this;
            transform.localPosition = value.Position;
        }
    }
    HexCell location;
    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }
}
