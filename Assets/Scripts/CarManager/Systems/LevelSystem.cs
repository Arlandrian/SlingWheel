using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFinishedEventArgs : EventArgs
{
    public int NewLevel { get; private set; }

    public LevelFinishedEventArgs(int newLevel)
    {
        NewLevel = newLevel;
    }
}

public class LevelSystem : MonoBehaviour
{
    public event Action<LevelFinishedEventArgs> LevelFinishedEvent;

    [SerializeField, TagSelector]
    private string LEVEL_FINISH_TAG = "LevelFinish";

    private int _currentLevel = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(LEVEL_FINISH_TAG))
        {
            _currentLevel++;
            LevelFinishedEvent?.Invoke(new LevelFinishedEventArgs(_currentLevel));
        }
    }
}
