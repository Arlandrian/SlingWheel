using SlingWheel;
using SlingWheel.Utils;
using System;
using UnityEngine;

public class CarController : MonoBehaviour
{
    #region Events

    public event Action<CheckPointReachedEventArgs> CheckPointReachedEvent;
    public event Action FailedEvent;
    public event Action<LevelFinishedEventArgs> LevelFinishedEvent;

    #endregion

    #region Private Variables

    private CheckPointSystem _checkPointSystem;
    private LevelSystem _levelSystem;
    private FailDetector _failDetector;
    private CarMovement _carMovement;

    #endregion

    public void Init()
    {
        GameManager.Instance.GameStartedEvent += OnGameStarted;
        _carMovement = GetComponent<CarMovement>();
        _carMovement.enabled = false;
        _carMovement.Init();
        RegisterDetectors();
    }
    
    private void RegisterDetectors()
    {
        _checkPointSystem = GetComponent<CheckPointSystem>();
        _checkPointSystem.CheckPointReachedEvent += OnCheckPointReached;

        _failDetector = GetComponent<FailDetector>();
        _failDetector.FailEvent += OnFailed;

        _levelSystem = GetComponent<LevelSystem>();
        _levelSystem.LevelFinishedEvent += OnLevelFinished;
    }

    #region Event Receivers

    private void OnGameStarted()
    {
        _carMovement.enabled = true;
    }

    private void OnCheckPointReached(CheckPointReachedEventArgs args)
    {
        CheckPointReachedEvent?.Invoke(args);
    }

    private void OnLevelFinished(LevelFinishedEventArgs args)
    {
        LevelFinishedEvent?.Invoke(args);
    }

    private void OnFailed()
    {
        _carMovement.enabled = false;
        FailedEvent?.Invoke();
    }

    #endregion
}
