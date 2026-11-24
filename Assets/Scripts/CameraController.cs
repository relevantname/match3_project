using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEventSystem;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float minOrthoSize = 5f;
    [SerializeField] private float offset = 0.75f;
    private Camera _mainCamera;

    private void Awake()
    {
        _mainCamera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        EventManager.Get<LevelInitialized>().AddListener(OnLevelInitialized);
    }

    private void OnDisable()
    {
        EventManager.Get<LevelInitialized>().RemoveListener(OnLevelInitialized);
    }

    private void OnLevelInitialized(Level level)
    {
        var gridSize = level.GameGrid.CurrentGridSize;
        var aspectRatio = (float)Screen.width / Screen.height;
        var orthoSizeByHeight = gridSize.y / 2f;
        var orthoSizeByWidth = (gridSize.x / 2f) / aspectRatio;
        _mainCamera.orthographicSize = Mathf.Max(orthoSizeByHeight, orthoSizeByWidth, minOrthoSize) + offset;
    }

    [ContextMenu("CamSize")]
    private void GetCamSize()
    {
        Debug.Log($"{Screen.width} {Screen.height}");
    }
}
