using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridObjectBehaviour : MonoBehaviour
{
    public abstract void ExecuteBehaviour(GameGrid gameGrid, IGridObjectBehaviourProperties properties = null);
}

public interface IGridObjectBehaviourProperties{}

public class AdjacentReactiveObjectProperties : IGridObjectBehaviourProperties
{
    public GridObjectId adjacentGridId;
}