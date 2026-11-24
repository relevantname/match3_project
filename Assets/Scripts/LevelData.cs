using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject
{
    [Tooltip("Do not modify manually!")] public List<GridObjectId> gridObjectIds;
    [Tooltip("Do not modify manually!")] public int columns;
    [Tooltip("Do not modify manually!")] public int rows;
    public List<LevelGoal> levelGoals;
    public int moveAmount;
}

[System.Serializable]
public class LevelGoal
{
    public GridObjectId goalObjectId;
    public int goalAmount;
}