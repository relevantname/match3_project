using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyNearbyGridObjectBehaviour : GridObjectBehaviour
{
    [SerializeField] private GridObjectId objectId;
    
    public override void ExecuteBehaviour(GameGrid gameGrid, IGridObjectBehaviourProperties properties = null)
    {
        var cellArrayCoord = gameGrid.GetGridCellCoordsFromWorldPosition(this.transform.position);

        var right = gameGrid.GetGridCell(new Vector2Int(cellArrayCoord.x + 1, cellArrayCoord.y));
        if(right != null && right.id == objectId) gameGrid.DestroyCellObjects(new List<GameGrid.GridCell>(){right});
        
        var left = gameGrid.GetGridCell(new Vector2Int(cellArrayCoord.x - 1, cellArrayCoord.y));
        if(left != null && left.id == objectId) gameGrid.DestroyCellObjects(new List<GameGrid.GridCell>(){left});
        
        var up = gameGrid.GetGridCell(new Vector2Int(cellArrayCoord.x, cellArrayCoord.y + 1));
        if(up != null && up.id == objectId) gameGrid.DestroyCellObjects(new List<GameGrid.GridCell>(){up});
        
        var down = gameGrid.GetGridCell(new Vector2Int(cellArrayCoord.x, cellArrayCoord.y -1));
        if(down != null && down.id == objectId) gameGrid.DestroyCellObjects(new List<GameGrid.GridCell>(){down});
    }
}
