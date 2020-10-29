using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    #region References
    [Header("Prefab References")]
    [SerializeField]
    private GameObject UIManagerPrefab;
    [SerializeField]
    private GameObject RoadManagerPrefab;
    [SerializeField]
    private GameObject CarPrefab;

    #endregion 

    #region Events

    public event Action GameStartedEvent;
    public event Action<LevelFinishedEventArgs> LevelFinishedEvent;
    public event Action FailedEvent;

    #endregion

    #region Private Variables

    private Camera _camera;
    private CameraFollow _cameraFollow;
    private UIManager _UIManager;
    private RoadManager _roadManager;
    private CarController _carController;

    public bool IsGameStarted { get; private set; } = false;
    private int _progressCounter = 0;
    private int _currentLevel = 1;

#if UNITY_EDITOR || UNITY_STANDALONE
    private bool IsTouchUp => Input.GetMouseButtonUp(0);
#else
    private bool IsTouchUp => Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended;
#endif

    #endregion

    private void Start()
    {
        _camera = Camera.main;
        _cameraFollow = _camera.GetComponent<CameraFollow>();

        RegisterUIManager();

        _roadManager = Instantiate(RoadManagerPrefab).GetComponent<RoadManager>();
        _roadManager.Init();

        LoadCar();
    }

    private void RegisterUIManager()
    {
        _UIManager = Instantiate(UIManagerPrefab).GetComponent<UIManager>();
        _UIManager.Init();
        _UIManager.RetryClickedEvent += () => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void LoadCar()
    {
        Vector3 carStartPosition = _roadManager.GetStartingLine().position - _roadManager.GetStartingLine().forward * 10;
        carStartPosition.y = 1.21f;
        GameObject carGameObject = Instantiate(CarPrefab, carStartPosition, _roadManager.GetStartingLine().rotation);
        _carController = carGameObject.GetComponent<CarController>();
        _carController.Init();

        _carController.FailedEvent += OnFailed;
        _carController.CheckPointReachedEvent += OnCheckPointReached;
        _carController.LevelFinishedEvent += OnLevelFinished;

        _cameraFollow.Init(carGameObject.transform);
    }

    private void Update()
    {
        if(IsGameStarted)
            return;
        if(IsTouchUp)
        {
            OnFirstTouch();
        }
    }

    #region Private Functions

    private void OnFirstTouch()
    {
        StartGame();
    }

    private void StartGame()
    {
        IsGameStarted = true;
        GameStartedEvent?.Invoke();
    }

    private void OnCheckPointReached(CheckPointReachedEventArgs args)
    {
        _progressCounter = args.CheckPointNumber;
        _UIManager.OnCheckPointReached(_progressCounter);
    }

    private void OnLevelFinished(LevelFinishedEventArgs args)
    {
        _currentLevel = args.NewLevel;
        LevelFinishedEvent?.Invoke(args);
    }

    private void OnFailed()
    {
        FailedEvent?.Invoke();
    }

    #endregion
}
