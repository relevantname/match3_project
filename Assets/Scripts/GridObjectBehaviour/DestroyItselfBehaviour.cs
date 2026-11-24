using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyItselfBehaviour : GridObjectBehaviour
{
    public override void ExecuteBehaviour(GameGrid gameGrid, IGridObjectBehaviourProperties properties = null)
    {
        var cellArrayCoord = gameGrid.GetGridCellCoordsFromWorldPosition(this.transform.position);
        gameGrid.DestroyCellObjects(new List<GameGrid.GridCell>() {gameGrid.GetGridCell(cellArrayCoord)});
    }
}
