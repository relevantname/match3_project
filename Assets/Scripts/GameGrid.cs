using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalEventSystem;
using UnityEngine;

public class GameGrid : MonoBehaviour
{
    [SerializeField] private SpriteRenderer gridFrameRenderer;
    [SerializeField] private Vector2 gridFrameDefaultSize = new Vector2(1.22f, 1.42f);
    [SerializeField] private float fallFirstSpawnOffset = 1f;
    [SerializeField] private float fallSpeed = 5f;
    [SerializeField] private int adjacentCellNeedForMatch = 2;
    [SerializeField] private List<MatchObjectCreationLogic> matchObjectCreationLogics = new();
    private GridObjectLibrary _gridObjectLibrary;
    private GridCell[,] _gridMap;
    private int _currentFallingObjects;
    private GameGridActionController _gridActionController;

    public GameGridActionController GridActionController => _gridActionController;
    public event Action GridMapInitialized;
    public Vector2 CurrentGridSize => gridFrameRenderer == null ? Vector2.zero : gridFrameRenderer.size;
    public int AdjacentCellNeedForMatch => adjacentCellNeedForMatch;

    private void Awake()
    {
        _gridActionController = GetComponent<GameGridActionController>();
        matchObjectCreationLogics = matchObjectCreationLogics.OrderByDescending(m => m.cellDestroyNeed).ToList();
    }

    private void OnEnable()
    {
        _gridActionController.AllActionsAreCompleted += OnAllActionsAreCompleted;
    }

    private void OnDisable()
    {
        _gridActionController.AllActionsAreCompleted -= OnAllActionsAreCompleted;
    }

    public void Init(LevelData levelData, GridObjectLibrary gridObjectLibrary)
    {
        _gridObjectLibrary = gridObjectLibrary;
        
        _gridMap = new GridCell[levelData.columns, levelData.rows];
        
        // Calculate grid offset to center the grid at (0, 0, 0)
        float gridWidth = levelData.columns;
        float gridHeight = levelData.rows;
        
        // Subtract 0.5f because 1 cell size is 1f.
        var gridOffset = new Vector3(gridWidth / 2f - 0.5f, gridHeight / 2f - 0.5f, 0f);
        
        for (var x = 0; x < levelData.columns; x++)
        {
            for (var y = 0; y < levelData.rows; y++)
            {
                var cell = new GridCell();
                
                var cellPosition = new Vector3(x, y, 0f) - gridOffset;
                cell.cellWorldPosition = cellPosition;
                
                var gridObjectId = levelData.gridObjectIds[y * levelData.columns + x];
                var gridGo = Instantiate(gridObjectLibrary.GetGridObjectDataByIdFromDict(gridObjectId).objectPrefab, cellPosition, Quaternion.identity, this.transform);
                var gridObj = gridGo.GetComponent<GridObject>();
                gridObj.SetSpriteLayerOrder(1+y);
                cell.SetGridObject(gridObj);
                cell.cellArrayCoord = new Vector2Int(x, y);
                
                _gridMap[x, y] = cell;
            }
        }
        
        CalculateGridFrameSize();
        
        GridMapInitialized?.Invoke();
    }

    private void CalculateGridFrameSize()
    {
        Debug.Log(_gridMap.GetLength(0));
        gridFrameRenderer.size = new Vector2(_gridMap.GetLength(0) + (gridFrameDefaultSize.x - 1f),
            _gridMap.GetLength(1) + (gridFrameDefaultSize.y - 1f));
    }
    
    // Method to get grid cell array location based on given world position
    public Vector2Int GetGridCellCoordsFromWorldPosition(Vector3 worldPosition)
    {
        float gridWidth = _gridMap.GetLength(0);
        float gridHeight = _gridMap.GetLength(1);
        Vector3 gridOffset = new Vector3(gridWidth / 2f - 0.5f, gridHeight / 2f - 0.5f, 0f);

        Vector3 localPosition = worldPosition + gridOffset;

        int x = Mathf.FloorToInt(localPosition.x);
        int y = Mathf.FloorToInt(localPosition.y);

        if (x >= 0 && x < _gridMap.GetLength(0) && y >= 0 && y < _gridMap.GetLength(1))
        {
            return new Vector2Int(x, y);
        }
        else
        {
            Debug.LogWarning("Position is out of bounds of the grid.");
            return new Vector2Int(-1, -1);
        }
    }
    
    // Method to detect adjacent cells for match
    public List<GridCell> FloodFill(int startX, int startY, GridObjectId targetId)
    {
        List<GridCell> matchingCells = new List<GridCell>();
        bool[,] visited = new bool[_gridMap.GetLength(0), _gridMap.GetLength(1)];
        
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            int x = current.x;
            int y = current.y;

            if (x < 0 || x >= _gridMap.GetLength(0) || y < 0 || y >= _gridMap.GetLength(1) || visited[x, y])
            {
                continue;
            }

            if (_gridMap[x, y].gridObject == null || _gridMap[x, y].gridObject.GridObjectData.id != targetId)
                continue;

            visited[x, y] = true;

            matchingCells.Add(_gridMap[x, y]);

            stack.Push(new Vector2Int(x + 1, y)); // Right
            stack.Push(new Vector2Int(x - 1, y)); // Left
            stack.Push(new Vector2Int(x, y + 1)); // Up
            stack.Push(new Vector2Int(x, y - 1)); // Down
        }

        return matchingCells;
    }

    public void Fill()
    {
        for (var x = 0; x < _gridMap.GetLength(0); x++)
        {
            for (var y = 0; y < _gridMap.GetLength(1); y++)
            {
                var cell = _gridMap[x, y];
                if (cell.id != GridObjectId.None)
                {
                    for (var k = 0; k < y; k++)
                    {
                        var cell2 = _gridMap[x, k];
                        if (cell2.id == GridObjectId.None)
                        {
                            SwapCellGridObjects(cell, cell2);
                            var obj = cell2.gridObject;
                            obj.MoveToPosition(cell2.cellWorldPosition, fallSpeed);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void SwapCellGridObjects(GridCell cell1, GridCell cell2)
    {
        var tempObj = cell1.gridObject;
        cell1.SetGridObject(cell2.gridObject);
        cell2.SetGridObject(tempObj);
    }

    public void Fall()
    {
        _currentFallingObjects = 0;
        for (var x = 0; x < _gridMap.GetLength(0); x++)
        {
            var emptyCells = GetEmptyCellsInColumn(x);
            if(emptyCells.Count <= 0) continue;
            var lastCellInCol = _gridMap[x, _gridMap.GetLength(1) - 1];
            for (var i = 0; i < emptyCells.Count; i++)
            {
                var emptyCell = emptyCells[i];
                var spawnPos = lastCellInCol.cellWorldPosition + Vector3.up * (fallFirstSpawnOffset + i);
                var gridGo = Instantiate(_gridObjectLibrary.GetRandomGridObjectData().objectPrefab, spawnPos, Quaternion.identity, this.transform);
                var gridObj = gridGo.GetComponent<GridObject>();
                gridObj.SetSpriteLayerOrder(1 + emptyCell.cellArrayCoord.y);
                emptyCell.SetGridObject(gridObj);
                _currentFallingObjects++;
                gridObj.MoveToPosition(emptyCell.cellWorldPosition, fallSpeed, OnObjectFallCompleted);
            }
        }
        
        ReOrderRenderLayers();

        if(_currentFallingObjects <= 0)
            EventManager.Get<GridFallCompleted>().Execute(this);
    }

    private void ReOrderRenderLayers()
    {
        for (var x = 0; x < _gridMap.GetLength(0); x++)
        {
            for (var y = 0; y < _gridMap.GetLength(1); y++)
            {
                var cell = _gridMap[x, y];
                if(cell == null || cell.id == GridObjectId.None) continue;
                cell.gridObject.SetSpriteLayerOrder(y + 1);
            }
        }
    }
    
    private void OnObjectFallCompleted(GridObject obj)
    {
        _currentFallingObjects--;
        if (_currentFallingObjects == 0)
        {
            Debug.Log("Grid Fall Completed!");
            EventManager.Get<GridFallCompleted>().Execute(this);
        }
    }

    private List<GridCell> GetEmptyCellsInColumn(int col)
    {
        var gridCells = new List<GridCell>();
        for (var y = 0; y < _gridMap.GetLength(1); y++)
        {
            var cell = _gridMap[col, y];
            if(cell.id == GridObjectId.None)
                gridCells.Add(cell);
        }
        return gridCells;
    }

    public void DestroyCellObjects(List<GridCell> cells)
    {
        foreach (var gridCell in cells)
        {
            NotifyAdjacentObjectsForDestroy(gridCell.cellArrayCoord, gridCell.id);
            gridCell.gridObject.DestroyGridObject(this);
            gridCell.SetGridObject(null);
        }
    }
    
    private void NotifyAdjacentObjectsForDestroy(Vector2Int coord, GridObjectId destroyedObjId)
    {
        var adjacentPositions = new Vector2Int[]
        {
            new(coord.x, coord.y + 1), // Up
            new(coord.x, coord.y - 1), // Down
            new(coord.x + 1, coord.y), // Right
            new(coord.x - 1, coord.y)  // Left
        };

        foreach (var adjacentPos in adjacentPositions)
        {
            var adjacentCell = GetGridCell(adjacentPos);
            if (adjacentCell == null || adjacentCell.gridObject == null) continue;
            var adjReactiveObj = adjacentCell.gridObject.GetComponent<AdjacentReactiveGridObject>();
            if(adjReactiveObj) adjReactiveObj.AdjacentGridObjectDestroyed(this, destroyedObjId);
        }
    }

    public GridCell GetGridCell(Vector2Int arrayCoord)
    {
        if (arrayCoord.x >= 0 && arrayCoord.x < _gridMap.GetLength(0) &&
            arrayCoord.y >= 0 && arrayCoord.y < _gridMap.GetLength(1))
        {
            return _gridMap[arrayCoord.x, arrayCoord.y];
        }
        return null;
    }

    public void CreateGridObjectOnCell(GridCell cell, GridObjectId objectId)
    {
        var gridGo = Instantiate(_gridObjectLibrary.GetGridObjectDataByIdFromDict(objectId).objectPrefab, cell.cellWorldPosition, Quaternion.identity, this.transform);
        var gridObj = gridGo.GetComponent<GridObject>();
        gridObj.SetSpriteLayerOrder(cell.cellArrayCoord.y + 1);
        cell.SetGridObject(gridObj);
    }

    public GridObjectId GetSuitableObjectId(int destroyedCells)
    {
        MatchObjectCreationLogic bestMatch = null;

        foreach (var logic in matchObjectCreationLogics)
        {
            if (logic.cellDestroyNeed <= destroyedCells)
            {
                if (bestMatch == null || logic.cellDestroyNeed > bestMatch.cellDestroyNeed)
                    bestMatch = logic;
            }
        }
        return bestMatch?.objectIdToCreate ?? GridObjectId.None;
    }
    
    public List<GridCell> GetCellsInDirection(Vector2Int startCoord, Vector2Int direction)
    {
        var cellsInDirection = new List<GridCell>();

        var gridWidth = _gridMap.GetLength(0);
        var gridHeight = _gridMap.GetLength(1);

        var currentCoord = startCoord + direction;

        while (currentCoord.x >= 0 && currentCoord.x < gridWidth && currentCoord.y >= 0 && currentCoord.y < gridHeight)
        {
            var cell = _gridMap[currentCoord.x, currentCoord.y];
            cellsInDirection.Add(cell);
            currentCoord += direction;
        }
        return cellsInDirection;
    }
    
    private void OnAllActionsAreCompleted()
    {
        Fill();
        Fall();
    }
    
    private void OnDrawGizmos()
    {
        if (_gridMap == null) return;

        Gizmos.color = Color.green;
        for (var x = 0; x < _gridMap.GetLength(0); x++)
        {
            for (var y = 0; y < _gridMap.GetLength(1); y++)
            {
                if (_gridMap[x, y] != null)
                    Gizmos.DrawWireCube(_gridMap[x, y].cellWorldPosition, new Vector3(1, 1, 0.1f));
            }
        }
    }

    public class GridCell
    {
        public GridObjectId id;
        public GridObject gridObject;
        public Vector3 cellWorldPosition;
        public Vector2Int cellArrayCoord;

        public void SetGridObject(GridObject gridObject)
        {
            this.id = gridObject == null ? GridObjectId.None : gridObject.GridObjectData.id;
            this.gridObject = gridObject;
            
            if(this.gridObject != null) this.gridObject.SetSpriteLayerOrder(cellArrayCoord.y + 1);
        }
    }

    [System.Serializable]
    public class MatchObjectCreationLogic
    {
        public int cellDestroyNeed;
        public GridObjectId objectIdToCreate;
    }
}
