using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexEffect : MonoBehaviour
{
    public int effectMovement;
    public string effectName;
    public static HexProjectile effectPrefab;
    public List<HexCell> pathToTravel;
    public float travelSpeed;
    public float rotationSpeed = 600f;

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
                location.Effect = null;
            }
            location = value;
            value.Effect = this;
            transform.localPosition = value.Position;
        }
    }
    HexCell location;

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }
    float orientation;
    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }
}
