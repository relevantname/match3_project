using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableGridObject : MonoBehaviour
{
    [SerializeField] private List<GridObjectBehaviour> clickBehaviours = new();

    public void Click(GameGrid gameGrid)
    {
        foreach (var gridObjectBehaviour in clickBehaviours)
            gridObjectBehaviour.ExecuteBehaviour(gameGrid);
    }
}
