using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RocketBehaviour : GridObjectBehaviour
{
    [SerializeField] private float rocketSpeed = 0.2f;
    [SerializeField] private Transform leftRocket;
    [SerializeField] private Transform rightRocket;
    private Vector3 _lastCheckedPosition1;
    private Vector3 _lastCheckedPosition2;
    
    public override void ExecuteBehaviour(GameGrid gameGrid, IGridObjectBehaviourProperties properties = null)
    {
        var rocketObj = GetComponent<RocketGridObject>();
        if (rocketObj == null) return;
        
        // if is vertical left rocket moves up
        var isVertical = rocketObj.IsVertical;

        var startCoord = gameGrid.GetGridCellCoordsFromWorldPosition(this.transform.position);
        var dir1 = isVertical ? Vector2Int.up : Vector2Int.right;
        var dir2 = isVertical ? Vector2Int.down : Vector2Int.left;

        var cellsInDir1 = gameGrid.GetCellsInDirection(startCoord, dir1);
        var cellsInDir2 = gameGrid.GetCellsInDirection(startCoord, dir2);

        leftRocket.transform.SetParent(null, true);
        rightRocket.transform.SetParent(null, true);
        DestroyObjectInCell(gameGrid.GetGridCell(startCoord), gameGrid);
        
        MoveAndDestroy(cellsInDir1, rightRocket, gameGrid);
        MoveAndDestroy(cellsInDir2, leftRocket, gameGrid);
    }

    private void MoveAndDestroy(List<GameGrid.GridCell> cells, Transform rocket, GameGrid gameGrid)
    {
        if (cells.Count <= 0)
        {
            Destroy(rocket.gameObject);
            return;
        }

        var lastCheckPoint = rocket.transform.position;
        gameGrid.GridActionController.RegisterAction();
        var lastCell = cells[^1];
        rocket.DOMove(lastCell.cellWorldPosition, rocketSpeed)
            .SetEase(Ease.Linear)
            .SetSpeedBased(true)
            .OnUpdate(() =>
            {
                foreach (var cell in cells)
                {
                    if (HasPassedObject(lastCheckPoint, rocket.transform.position, cell.cellWorldPosition))
                    {
                        DestroyObjectInCell(cell, gameGrid);
                    }
                }

                lastCheckPoint = rocket.transform.position;
            })
            .OnComplete(() =>
            {
                Destroy(rocket.gameObject);
                gameGrid.GridActionController.ActionCompleted();
            });
    }
    
    private bool HasPassedObject(Vector3 lastPosition, Vector3 currentPosition, Vector3 objectPosition)
    {
        return Vector3.Distance(lastPosition, objectPosition) + Vector3.Distance(currentPosition, objectPosition)
               <= Vector3.Distance(lastPosition, currentPosition) + 0.01f;
    }

    private void DestroyObjectInCell(GameGrid.GridCell cell, GameGrid grid)
    {
        if (cell.gridObject == null) return;
        cell.gridObject.DestroyGridObject(grid);
        cell.SetGridObject(null);
    }
}
