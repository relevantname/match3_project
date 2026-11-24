using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAdjacentCellsBehaviour : GridObjectBehaviour
{
    [SerializeField] private GridObjectId objectId;
    [SerializeField] private int cellNeed = 2; 
    
    public override void ExecuteBehaviour(GameGrid gameGrid, IGridObjectBehaviourProperties properties = null)
    {
        var cellArrayCoord = gameGrid.GetGridCellCoordsFromWorldPosition(this.transform.position);
        var cells = gameGrid.FloodFill(cellArrayCoord.x, cellArrayCoord.y, objectId);
        if (cells.Count >= cellNeed)
        {
            gameGrid.GridActionController.RegisterAction();
            gameGrid.DestroyCellObjects(cells);
            var objectToCreateId = gameGrid.GetSuitableObjectId(cells.Count);
            if(objectToCreateId != GridObjectId.None)
                gameGrid.CreateGridObjectOnCell(gameGrid.GetGridCell(cellArrayCoord), objectToCreateId);
            gameGrid.GridActionController.ActionCompleted();
        }
    }
}
