using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjacentReactiveGridObject : MonoBehaviour
{
    [SerializeField] private List<GridObjectBehaviour> adjacentDestroyReactionBehaviours = new();

    public void AdjacentGridObjectDestroyed(GameGrid gameGrid, GridObjectId destroyedObjectId)
    {
        foreach (var behaviour in adjacentDestroyReactionBehaviours)
            behaviour.ExecuteBehaviour(gameGrid, new AdjacentReactiveObjectProperties(){adjacentGridId = destroyedObjectId});
    }
}
