using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnFallToGroundBehaviour : GridObjectBehaviour
{
    public override void ExecuteBehaviour(GameGrid gameGrid, IGridObjectBehaviourProperties properties = null)
    {
        var cellArrayCoord = gameGrid.GetGridCellCoordsFromWorldPosition(this.transform.position);
        if (cellArrayCoord.y <= 0)
        {
            var cell = gameGrid.GetGridCell(cellArrayCoord);
            if (cell.gridObject != null)
            {
                gameGrid.GridActionController.RegisterAction();
                cell.gridObject.DestroyGridObject(gameGrid);
                cell.SetGridObject(null);
                gameGrid.GridActionController.ActionCompleted();
            }
        }
    }
}
