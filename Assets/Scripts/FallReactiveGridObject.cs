using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEventSystem;
using UnityEngine;

public class FallReactiveGridObject : MonoBehaviour
{
    [SerializeField] private List<GridObjectBehaviour> fallBehaviours = new();

    private void OnEnable()
    {
        EventManager.Get<GridFallCompleted>().AddListener(OnGridFallCompleted);
    }

    private void OnDisable()
    {
        EventManager.Get<GridFallCompleted>().RemoveListener(OnGridFallCompleted);
    }

    private void OnGridFallCompleted(GameGrid gameGrid)
    {
        foreach (var behaviour in fallBehaviours)
            behaviour.ExecuteBehaviour(gameGrid);
    }
}
