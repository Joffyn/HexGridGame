using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexFire : MonoBehaviour
{
    public Transform firePrefab;
    Transform container;

    public void Clear()
    {
        if (container)
        {
            Destroy(container.gameObject);
        }
        container = new GameObject("Features Container").transform;
        container.SetParent(transform, false);
    }

    public void Apply()
    {

    }

    public void AddFire (HexCell cell, Vector3 position)
    {

        if(cell.FireLevel == 1 && !cell.IsUnderwater)
        {
        Transform instance = Instantiate(firePrefab);
        position.y += instance.localScale.y * 0.5f;
        instance.localPosition = HexMetrics.Perturb(position);
        instance.SetParent(container, false);
        }
    }
}
