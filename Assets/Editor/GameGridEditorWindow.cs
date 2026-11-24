using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameGridEditorWindow : EditorWindow
{
    private LevelData currentLevelData;
    private GridObjectLibrary gridObjectLibrary;
    
    // Grid settings
    private int gridSize = 50; // Default grid cell size
    private int columns = 10;  // Default number of columns
    private int rows = 10;     // Default number of rows

    private int tempColumns;
    private int tempRows;
    
    // 2D array to store object IDs for each grid cell
    private GridObjectId[,] gridObjects;
    
    private Dictionary<GridObjectId, bool> excludeGridObjectIds = new Dictionary<GridObjectId, bool>();
    private bool showExclusionList = false;
    private Vector2 exclusionListScrollPosition;

    // Selected object from the dropdown
    private GridObjectId selectedObject = GridObjectId.None;
    
    // Scroll position for the grid
    private Vector2 scrollPosition;

    [MenuItem("Window/Grid Editor")]
    public static void ShowWindow()
    {
        GetWindow<GameGridEditorWindow>("Grid Editor");
    }

    void OnEnable()
    {
        // Initialize the temporary values to match the actual grid
        tempColumns = columns;
        tempRows = rows;
        
        InitializeGrid();

        foreach (GridObjectId id in System.Enum.GetValues(typeof(GridObjectId)))
            excludeGridObjectIds[id] = false;
        excludeGridObjectIds[GridObjectId.None] = true;
    }

    void OnGUI()
    {
        // Create a section for UI controls (Grid settings and dropdown)
        GUILayout.BeginVertical("box");
        GUILayout.Label("Grid Settings", EditorStyles.boldLabel);

        // Fields for changing grid settings
        // gridObjectLibrary = AssetDatabase.LoadAssetAtPath<GridObjectLibrary>("Assets/Resources/GridItemLibrary.asset");
        gridObjectLibrary = AssetDatabase.LoadAssetAtPath<GridObjectLibrary>("Assets/ScriptableObjects/GridObjectLibrary.asset");
        gridSize = EditorGUILayout.IntField("Grid Size", gridSize);
        tempColumns = EditorGUILayout.IntField("Columns", tempColumns);
        tempRows = EditorGUILayout.IntField("Rows", tempRows);

        showExclusionList = EditorGUILayout.Foldout(showExclusionList, "Exclude GridObjectId(s) from Randomization");

        if (showExclusionList)
        {
            GUILayout.BeginVertical("box");
            exclusionListScrollPosition = EditorGUILayout.BeginScrollView(exclusionListScrollPosition, GUILayout.Height(100)); // Add a scroll view
            foreach (var id in System.Enum.GetValues(typeof(GridObjectId)))
            {
                if((GridObjectId)id == GridObjectId.None) continue;
                excludeGridObjectIds[(GridObjectId)id] = EditorGUILayout.Toggle($"Exclude {id}", excludeGridObjectIds[(GridObjectId)id]);
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        
        // If grid settings change, reinitialize the grid
        if (GUILayout.Button("Update Grid"))
            UpdateGrid();
        
        if (GUILayout.Button("Clear Grid"))
            ClearGrid();
        
        if (GUILayout.Button("Randomize Grid"))
            RandomizeGrid();
        
        if (GUILayout.Button("Save Level"))
            SaveLevel();

        if (GUILayout.Button("Load Level"))
            LoadLevel();

        // Dropdown to select the object to place
        selectedObject = (GridObjectId)EditorGUILayout.EnumPopup("Select Grid Object", selectedObject);
        
        // Field to assign an existing LevelData asset
        currentLevelData = (LevelData)EditorGUILayout.ObjectField("Level Data", currentLevelData, typeof(LevelData), false);
        
        GUILayout.Space(10);

        GUILayout.EndVertical();

        // Leave space for the grid by adding a flexible space between controls and the grid
        GUILayout.Space(10);
        
        // Begin scroll view for the grid
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 200));

        // Make sure the grid does not overlap the UI elements above
        Rect gridRect = GUILayoutUtility.GetRect(columns * gridSize, rows * gridSize);

        DrawGrid(gridRect);
        HandleMouseClick(Event.current, gridRect);

        GUILayout.EndScrollView();  // End the scroll view
        
        // Repaint the window to keep it interactive
        if (Event.current.type == EventType.MouseDown)
        {
            Repaint();
        }
    }
    
    // Update the gridObjects array while preserving existing data
    void UpdateGrid()
    {
        // Create a new grid based on the updated rows and columns
        GridObjectId[,] newGridObjects = new GridObjectId[tempColumns, tempRows];

        // Copy data from the existing gridObjects to the new grid, but only within bounds
        for (int x = 0; x < Mathf.Min(columns, tempColumns); x++)
        {
            for (int y = 0; y < Mathf.Min(rows, tempRows); y++)
            {
                newGridObjects[x, y] = gridObjects[x, y];
            }
        }

        // Replace the current grid with the updated grid
        gridObjects = newGridObjects;

        // Update the grid size
        columns = tempColumns;
        rows = tempRows;
    }
    
    private void ClearGrid()
    {
        // Iterate through each cell and set it to GridObjectId.None
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                gridObjects[x, y] = GridObjectId.None;
            }
        }

        // Refresh the editor window to update the grid visually
        Repaint();
    
        Debug.Log("Grid has been cleared.");
    }
    
    private void RandomizeGrid()
    {
        // First, clear the grid
        ClearGrid();

        // Get all possible GridObjectId values excluding the ones marked as excluded
        List<GridObjectId> availableIds = new List<GridObjectId>();

        foreach (var id in System.Enum.GetValues(typeof(GridObjectId)))
        {
            if (!excludeGridObjectIds[(GridObjectId)id])
                availableIds.Add((GridObjectId)id);
        }

        // Randomly assign GridObjectId to each cell in the grid
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (availableIds.Count > 0)
                    gridObjects[x, y] = availableIds[Random.Range(1, availableIds.Count)];
                else gridObjects[x, y] = GridObjectId.None;
            }
        }

        // Refresh the editor window to update the grid visually
        Repaint();

        Debug.Log("Grid has been randomized.");
    }

    // Initialize the gridObjects array with the selected columns and rows
    void InitializeGrid()
    {
        gridObjects = new GridObjectId[columns, rows];

        // Initialize all grid cells with None (no object assigned)
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                gridObjects[x, y] = GridObjectId.None;
            }
        }
    }

    private void DrawGrid(Rect rect)
    {
        // Calculate the maximum number of cells that can fit in the window, based on the gridSize
        float totalGridWidth = columns * gridSize;
        float totalGridHeight = rows * gridSize;

        // Ensure the grid doesn't exceed the provided window space
        if (totalGridWidth > rect.width) totalGridWidth = rect.width;
        if (totalGridHeight > rect.height) totalGridHeight = rect.height;

        // Draw each cell in the grid
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Rect cellRect = new Rect(rect.x + x * gridSize, rect.y + y * gridSize, gridSize, gridSize);
                if(gridObjectLibrary == null || gridObjects[x, y] == GridObjectId.None)
                    GUI.Box(cellRect, gridObjects[x, y].ToString()); // Display the object in the grid
                else
                {
                    var gridObjectData = gridObjectLibrary.GetGridObjectDataByIdFromDict(gridObjects[x, y]);
                    if (gridObjectData == null || gridObjectData.objectSprite == null)
                    {
                        GUI.Box(cellRect, gridObjects[x, y].ToString());
                    }
                    else
                    {
                        var tex = gridObjectData.objectSprite.texture;
                        // Define the rect to draw the texture within
                        Rect spriteRect = new Rect(cellRect.x, cellRect.y, gridSize, gridSize); // x, y, width, height
                        // Draw the sprite's texture
                        GUI.DrawTexture(spriteRect, tex);   
                    }
                }
            }
        }
    }

    private void HandleMouseClick(Event e, Rect gridRect)
    {
        if (e.type == EventType.MouseDown && e.button == 0) // Left-click
        {
            Vector2 mousePos = e.mousePosition;

            // Check if the click is inside the gridRect
            if (!gridRect.Contains(mousePos)) return;

            // Calculate the grid cell that was clicked based on gridSize
            int gridX = Mathf.FloorToInt((mousePos.x - gridRect.x) / gridSize);
            int gridY = Mathf.FloorToInt((mousePos.y - gridRect.y) / gridSize);

            if (gridX >= 0 && gridY >= 0 && gridX < columns && gridY < rows)
            {
                // Assign the selected object to the clicked grid cell
                gridObjects[gridX, gridY] = selectedObject;
                Debug.Log($"Assigned {selectedObject} to Grid Cell: {gridX}, {gridY}");
            }
        }
    }

    #region Save/Load

    // Save the current grid state to a LevelData ScriptableObject
    private void SaveLevel()
    {
        if (currentLevelData == null)
        {
            // If no LevelData is assigned, create a new one
            string path = EditorUtility.SaveFilePanelInProject("Save Level", "New LevelData", "asset", "Save your level data");

            if (string.IsNullOrEmpty(path)) return;

            currentLevelData = CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(currentLevelData, path);
        }

        // Update the LevelData with the current grid state
        currentLevelData.columns = columns;
        currentLevelData.rows = rows;

        // Flatten the 2D array into a list
        currentLevelData.gridObjectIds = new List<GridObjectId>();
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                currentLevelData.gridObjectIds.Add(gridObjects[x, y]);
            }
        }

        // Save changes to the assigned or newly created asset
        EditorUtility.SetDirty(currentLevelData);
        AssetDatabase.SaveAssets();

        Debug.Log("Level saved.");
    }

    // Load a LevelData ScriptableObject
    private void LoadLevel()
    {
        string path = EditorUtility.OpenFilePanel("Load Level", "Assets", "asset");

        if (string.IsNullOrEmpty(path)) return;

        path = FileUtil.GetProjectRelativePath(path);

        currentLevelData = AssetDatabase.LoadAssetAtPath<LevelData>(path);

        if (currentLevelData != null)
        {
            columns = currentLevelData.columns;
            rows = currentLevelData.rows;

            gridObjects = new GridObjectId[columns, rows];
            int index = 0;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    gridObjects[x, y] = currentLevelData.gridObjectIds[index];
                    index++;
                }
            }
            
            // Update the temporary values for columns and rows to reflect the loaded data
            tempColumns = columns;
            tempRows = rows;

            Debug.Log("Level loaded from: " + path);

            // Repaint the window to ensure the grid updates visually
            Repaint();
        }
        else
        {
            Debug.LogError("Failed to load LevelData.");
        }
    }
    
    #endregion
}
