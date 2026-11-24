
using System;
using System.Collections.Generic;
using GlobalEventSystem;
using UnityEngine;

public class Level : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private GameGrid gameGrid;

    public LevelData LevelData => levelData;
    public GameGrid GameGrid => gameGrid;

    private int _remainingMoveAmount;
    private List<LevelGoal> _levelGoals = new();

    public int RemainingMoveAmount => _remainingMoveAmount;
    public List<LevelGoal> LevelGoals => _levelGoals;

    private void Awake()
    {
        if (gameGrid == null) return;
        gameGrid.GridMapInitialized += OnGridMapInitialized;
        gameGrid.Init(levelData, GameDataManager.Instance.GridObjectLibrary);
        _remainingMoveAmount = levelData.moveAmount;
        _levelGoals = new List<LevelGoal>();
        foreach (var lg in levelData.levelGoals)
        {
            _levelGoals.Add(new LevelGoal(){goalAmount = lg.goalAmount, goalObjectId = lg.goalObjectId});
        }
    }

    private void OnEnable()
    {
        EventManager.Get<GridObjectDestroyed>().AddListener(OnGridObjectDestroyed);
        EventManager.Get<GridObjectClicked>().AddListener(OnGridObjectClicked);
        EventManager.Get<GridFallCompleted>().AddListener(OnGridFallCompleted);
    }

    private void OnDisable()
    {
        if(gameGrid) gameGrid.GridMapInitialized -= OnGridMapInitialized;
        EventManager.Get<GridObjectDestroyed>().RemoveListener(OnGridObjectDestroyed);
        EventManager.Get<GridObjectClicked>().RemoveListener(OnGridObjectClicked);
        EventManager.Get<GridFallCompleted>().RemoveListener(OnGridFallCompleted);
    }

    private void OnGridFallCompleted(GameGrid gameGrid)
    {
        if (CheckGoalStates())
        {
            Debug.Log("Level Completed!");
            LevelManager.Instance.MoveToNextLevel();
            return;
        }
        CheckRemainingMoveAmount();
    }

    private bool CheckGoalStates()
    {
        foreach (var goal in _levelGoals)
        {
            if (goal.goalAmount > 0)
                return false;
        }
        return true;
    }
    
    private void CheckRemainingMoveAmount()
    {
        if (_remainingMoveAmount <= 0)
        {
            Debug.Log("Level Failed!");
            LevelManager.Instance.ResetLevel();
            return;
        }
    }

    private void OnGridObjectClicked()
    {
        _remainingMoveAmount--;
        EventManager.Get<LevelStateChanged>().Execute(this);
    }

    private void OnGridObjectDestroyed(GridObjectId id, Vector3 pos)
    {
        var goal = _levelGoals.Find(g => g.goalObjectId == id);
        if (goal != null)
        {
            goal.goalAmount--;
            if (goal.goalAmount < 0) goal.goalAmount = 0;
            EventManager.Get<LevelStateChanged>().Execute(this);
        }
    }

    private void OnGridMapInitialized()
    {
        EventManager.Get<LevelInitialized>().Execute(this);
    }
}
