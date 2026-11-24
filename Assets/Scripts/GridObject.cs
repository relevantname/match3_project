using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GlobalEventSystem;
using UnityEngine;

public class GridObject : MonoBehaviour
{
    [SerializeField] private GridObjectData gridObjectData;
    [SerializeField] private List<GridObjectBehaviour> destroyBehaviours = new();
    private SpriteRenderer[] _objectRenderers;
    private Tween _moveTween;
    private bool _isAboutToDestroy;
    
    public GridObjectData GridObjectData => gridObjectData;

    public void SetSpriteLayerOrder(int order)
    {
        if (_objectRenderers == null)
            _objectRenderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var spriteRenderer in _objectRenderers)
            spriteRenderer.sortingOrder = order;
    }

    public void DestroyGridObject(GameGrid gameGrid)
    {
        if (_isAboutToDestroy) return;
        _isAboutToDestroy = true;
        foreach (var gridObjectBehaviour in destroyBehaviours)
            gridObjectBehaviour.ExecuteBehaviour(gameGrid);
        EventManager.Get<GridObjectDestroyed>().Execute(gridObjectData.id, transform.position);
        Destroy(this.gameObject);
    }
    
    public void MoveToPosition(Vector3 pos, float speed, Action<GridObject> iteMovementCompletedCallback = null)
    {
        _moveTween?.Kill();
        _moveTween = transform.DOMoveY(pos.y, speed)
            .SetSpeedBased(true)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                iteMovementCompletedCallback?.Invoke(this);
            });
    }
}
