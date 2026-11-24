using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))][ExecuteAlways] 
public class LevelBackgroundController : MonoBehaviour
{
    private void Start()
    {
        AdjustBgSizeAccordingToResolution();
    }

    [ContextMenu("Scale")]
    private void AdjustBgSizeAccordingToResolution()
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        var mainCamera = Camera.main;

        var screenHeight = mainCamera.orthographicSize * 2.0f;
        var screenWidth = screenHeight * mainCamera.aspect;

        var spriteSize = spriteRenderer.sprite.bounds.size;

        var scale = transform.localScale;
        scale.x = screenWidth / spriteSize.x;
        scale.y = screenHeight / spriteSize.y;
        
        transform.localScale = scale;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        AdjustBgSizeAccordingToResolution();      
    }
#endif
}
