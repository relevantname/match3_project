using System;
using System.Collections;
using System.Collections.Generic;
using GlobalEventSystem;
using UnityEngine;

public class LevelInputController : MonoBehaviour
{
    private Level _level;
    private GameGridActionController _gridActionController;
    
    private void Awake()
    {
        _level = GetComponent<Level>();
        _gridActionController = _level.GameGrid.GridActionController;
    }

    private void Update()
    {
        if (_gridActionController == null)
        {
            _gridActionController = _level.GameGrid.GridActionController;
            return;
        }

        if (_gridActionController.ActiveActions > 0) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            // Cast a ray from the camera through the mouse position in 2D
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
            // Perform the 2D raycast, detecting objects in the specified layer
            RaycastHit2D hitInfo = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, 1<<6);

            // Check if the ray hit a 2D collider
            if (hitInfo.collider != null)
            {
                var clickableGridObject = hitInfo.collider.GetComponent<ClickableGridObject>();
                if (clickableGridObject)
                {
                    clickableGridObject.Click(_level.GameGrid);
                    EventManager.Get<GridObjectClicked>().Execute();
                }
            }
        }
    }
}
