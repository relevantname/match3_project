using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEventSystem;
using TMPro;
using UnityEngine;

public class GamePanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text remainingMoveAmountText;
    [SerializeField] private Transform goalsHolder;
    [SerializeField] private GameObject goalHolderPrefab;

    private List<GoalHolder> _goalHolders = new();

    private void Awake()
    {
        remainingMoveAmountText.text = "";
    }

    private void OnEnable()
    {
        EventManager.Get<LevelInitialized>().AddListener(OnLevelInitialized);
        EventManager.Get<LevelStateChanged>().AddListener(OnLevelStateChanged);
    }

    private void OnDisable()
    {
        EventManager.Get<LevelInitialized>().RemoveListener(OnLevelInitialized);
        EventManager.Get<LevelStateChanged>().RemoveListener(OnLevelStateChanged);
    }

    private void OnLevelStateChanged(Level level)
    {
        remainingMoveAmountText.text = level.RemainingMoveAmount.ToString();
        foreach (var goalHolder in _goalHolders)
        {
            var goal = level.LevelGoals.Find(g => g.goalObjectId == goalHolder.ObjectId);
            if(goal != null) goalHolder.SetRemainingAmount(goal.goalAmount);
        }
    }

    private void OnLevelInitialized(Level level)
    {
        remainingMoveAmountText.text = level.LevelData.moveAmount.ToString();
        CreateGoalHolders(level.LevelData);
    }

    private void CreateGoalHolders(LevelData levelData)
    {
        foreach (var goalHolder in _goalHolders)
            Destroy(goalHolder.gameObject);
        _goalHolders.Clear();

        foreach (var goal in levelData.levelGoals)
        {
            var goalHolderGo = Instantiate(goalHolderPrefab, goalsHolder);
            goalHolderGo.transform.localPosition = Vector3.zero;
            goalHolderGo.transform.localScale = Vector3.one;
            var goalHolder = goalHolderGo.GetComponent<GoalHolder>();
            goalHolder.Init(goal);
            _goalHolders.Add(goalHolder);
        }
    }
}
