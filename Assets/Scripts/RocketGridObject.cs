using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RocketGridObject : MonoBehaviour
{
    private bool isVertical;

    public bool IsVertical => isVertical;
    
    private void OnEnable()
    {
        var rand = Random.Range(0, 2);
        if (rand == 1)
        {
            isVertical = true;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }
}
