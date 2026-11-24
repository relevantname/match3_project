using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridObjectLibrary", menuName = "GridSystem/GridObjectLibrary")]
public class GridObjectLibrary : ScriptableObject
{
    public List<GridObjectData> gridObjectData = new();
    public List<GridObjectId> fallableObjectIds = new();

    private Dictionary<GridObjectId, GridObjectData> _gridObjectDataDict;
    
    public GridObjectData GetGridObjectDataById(GridObjectId id)
    {
        return gridObjectData.Find(d => d.id == id);
    }
    
    public GridObjectData GetGridObjectDataByIdFromDict(GridObjectId id)
    {
        if (_gridObjectDataDict == null)
            InitDictionary();

        return _gridObjectDataDict != null && _gridObjectDataDict.TryGetValue(id, out var data) ? data : null;
    }

    public GridObjectData GetRandomGridObjectData()
    {
        var randId =  fallableObjectIds[Random.Range(0, fallableObjectIds.Count)];
        return GetGridObjectDataByIdFromDict(randId);
    }
    
    private void InitDictionary()
    {
        if (_gridObjectDataDict == null)
        {
            _gridObjectDataDict = new Dictionary<GridObjectId, GridObjectData>();
            foreach (var data in gridObjectData)
            {
                if (!_gridObjectDataDict.ContainsKey(data.id))
                    _gridObjectDataDict.Add(data.id, data);
            }
        }
    }
}
