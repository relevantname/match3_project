using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoalHolder : MonoBehaviour
{
    [SerializeField] private Image goalImage;
    [SerializeField] private TMP_Text remainingAmountText;
    private GridObjectId _objectId;

    public GridObjectId ObjectId => _objectId;

    public void Init(LevelGoal goal)
    {
        _objectId = goal.goalObjectId;
        goalImage.sprite = GameDataManager.Instance.GridObjectLibrary.GetGridObjectDataByIdFromDict(goal.goalObjectId)
            .objectSprite;
        remainingAmountText.text = $"{goal.goalAmount}";
    }
    
    public void SetRemainingAmount(int amount)
    {
        remainingAmountText.text = $"{amount}";
    }
}
