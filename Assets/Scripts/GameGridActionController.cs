using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameGridActionController : MonoBehaviour
{
    private int _activeActions = 0;
    public event Action AllActionsAreCompleted;

    public int ActiveActions => _activeActions;
    
    public void RegisterAction()
    {
        _activeActions++;
    }

    public void ActionCompleted()
    {
        _activeActions--;
        if (_activeActions <= 0)
            AllActionsAreCompleted?.Invoke();
    }
}
