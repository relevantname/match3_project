using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    [SerializeField] private List<Level> _levels = new();

    private int _currentLevelIndex;
    private Level _currentLevel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    private void Start()
    {
        _currentLevelIndex = PlayerPrefs.GetInt("level", 0);
        LoadLevel(_currentLevelIndex);
    }
    
    private void LoadLevel(int levelIndex)
    {
        if (levelIndex >= _levels.Count)
            levelIndex = 0;

        if(_currentLevel != null)
            Destroy(_currentLevel.gameObject);
        
        var levelGo = Instantiate(_levels[levelIndex].gameObject, Vector3.zero, Quaternion.identity);
        _currentLevel = levelGo.GetComponent<Level>();

        // Save the current level index to PlayerPrefs
        PlayerPrefs.SetInt("level", levelIndex);
        PlayerPrefs.Save();
    }

    public void ResetLevel()
    {
        LoadLevel(_currentLevelIndex);
    }
    
    public void MoveToNextLevel()
    {
        _currentLevelIndex++;
        LoadLevel(_currentLevelIndex);
    }
}
