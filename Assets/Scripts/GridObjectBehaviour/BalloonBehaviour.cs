using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonBehaviour : GridObjectBehaviour
{
    [SerializeField] private List<GridObjectId> objectIdsToCheck = new();

    
    public override void ExecuteBehaviour(GameGrid gameGrid, IGridObjectBehaviourProperties properties = null)
    {
        if (properties is not AdjacentReactiveObjectProperties adjacentProps) return;

        if (!objectIdsToCheck.Contains(adjacentProps.adjacentGridId)) return;
        
        gameGrid.GridActionController.RegisterAction();
        var cellArrayCoord = gameGrid.GetGridCellCoordsFromWorldPosition(this.transform.position);
        var cell = gameGrid.GetGridCell(cellArrayCoord);
        if (cell.gridObject != null)
        {
            cell.gridObject.DestroyGridObject(gameGrid);
            cell.SetGridObject(null);
        }

        gameGrid.GridActionController.ActionCompleted();
    }

    // private IEnumerator WaitOneFrameAndDestroy()
    // {
    //     yield return null;
    //     gameGrid.GridActionController.ActionCompleted();
    // }
}
