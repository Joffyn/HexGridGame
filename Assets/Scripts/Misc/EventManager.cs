using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void ShiftHeldDown();
    public static event ShiftHeldDown onShiftHeldDown;

    // Start is called before the first frame update
    void OnGUI()
    {
        if (Event.current.shift)
        {
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
