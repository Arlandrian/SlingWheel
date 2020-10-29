using SlingWheel;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

public class RoadManager : MonoBehaviour
{
    #region Prefab Refs

    [Header("Prefab References")]
    [SerializeField]
    private GameObject StraightPrefab;
    [SerializeField]
    private GameObject CornerPrefab;
    [SerializeField]
    private GameObject U_CornerPrefab;
    [SerializeField]
    private GameObject LevelFinishPrefab;
    [SerializeField]
    private GameObject StartingLinePrefab;

    #endregion

    #region RoadManager Configs
    [Header("RoadManager Configs")]
    [SerializeField]
    private int _numberOfCornersPerLevel = 10;

    [SerializeField]
    private int _cornerIncreasePerLevel = 2;

    [SerializeField,Range(0f,1f)]
    private float _UCornerChance = 0.5f;

    #endregion

    #region Public Variables

    public Transform GetStartingLine() => _startingLine;

    #endregion

    #region Private Variables

    private Queue<GameObject> _trackReleaseQueue;
    private int _lastGeneratedLevelNumber = 0;
    private Transform _lastGeneratedLevelEndTrack;
    private CornerType _lastCornerType;
    private Transform _startingLine;

    // if last generated corner made the road go up, next corner can be same direction otherwise it must! be the opposite of the last direction
    // this is neccessary because we should prevent overlaping roads
    private bool _isLastGoingUp;

    private int _generatedLevelCounter = 0;

    #endregion

    public void Init()
    {
        _numberOfCornersPerLevel = GlobalConfigs.CornerPerLevel;
        _cornerIncreasePerLevel = GlobalConfigs.CornerIncreasePerLevel;

        _trackReleaseQueue = new Queue<GameObject>();
        PoolManager.Instance.root = transform;
        PoolManager.Instance.SpawnObjectEvent += OnObjectSpawned;

        GenerateRoad();

        GameManager.Instance.LevelFinishedEvent += OnReachedNextLevel;
    }
    private void OnReachedNextLevel(LevelFinishedEventArgs args)
    {
        _generatedLevelCounter++;
        Debug.Log("New Level Generated: "+_generatedLevelCounter);
        _numberOfCornersPerLevel += _cornerIncreasePerLevel;
        _lastGeneratedLevelEndTrack = GenerateLevel(_lastGeneratedLevelEndTrack);
    }

    private void GenerateRoad()
    {
        // Put One Straigth
        GameObject straightObj1 = PoolManager.Instance.spawnObject(StraightPrefab);

        // Put One More Straight
        Vector3 pos = straightObj1.transform.Find("EndPoint").position;
        GameObject straightObj2 = PoolManager.Instance.spawnObject(StraightPrefab, pos, Quaternion.Euler(0,0,0));

        // Put starting line on second one (Just a visual thing)
        Vector3 startingLinePosition = straightObj2.transform.position + straightObj2.transform.forward * 15f;
        startingLinePosition.y = 0.01f;
        _startingLine = Instantiate(StartingLinePrefab, startingLinePosition, Quaternion.identity).transform;

        _lastGeneratedLevelEndTrack = straightObj2.transform;

        _isLastGoingUp = true;
        // Generate 3 Level
        for (int i = 0; i < 3; i++)
        {
            _lastGeneratedLevelEndTrack = GenerateLevel(_lastGeneratedLevelEndTrack);
        }
    }

    // Generates tracks for whole level
    private Transform GenerateLevel(Transform lastTrack)
    {
        Transform lastGeneratedTrack = lastTrack.transform;
        for (int i = 0; i < _numberOfCornersPerLevel; i++)
        {
            lastGeneratedTrack = GenerateCorner(lastGeneratedTrack);
            if(i < _numberOfCornersPerLevel - 1)
            {
                lastGeneratedTrack = GenerateStraight(lastGeneratedTrack);
            }
        }
        _lastGeneratedLevelNumber++;

        // Create a long road for level finished
        lastGeneratedTrack = GenerateLevelFinish(lastGeneratedTrack);

        return lastGeneratedTrack;
    }

    // Returns the end point of last generated track
    private Transform GenerateCorner(Transform lastGeneratedTrack)
    {
        Transform endpoint = lastGeneratedTrack.Find("EndPoint");
        GameObject cornerObj = null;
        CornerType nextCornerType = DecideCorner();

        switch (nextCornerType)
        {
            case CornerType.Left:
                cornerObj = PoolManager.Instance.spawnObject(CornerPrefab, endpoint.position, endpoint.rotation);
                cornerObj.transform.SetLocalScaleX(1);
                break;
            case CornerType.Right:
                cornerObj = PoolManager.Instance.spawnObject(CornerPrefab, endpoint.position, endpoint.rotation);
                cornerObj.transform.SetLocalScaleX(-1);
                break;
            case CornerType.ULeft:
                cornerObj = PoolManager.Instance.spawnObject(U_CornerPrefab, endpoint.position, endpoint.rotation);
                cornerObj.transform.SetLocalScaleX(1);
                break;
            case CornerType.URight:
                cornerObj = PoolManager.Instance.spawnObject(U_CornerPrefab, endpoint.position, endpoint.rotation);
                cornerObj.transform.SetLocalScaleX(-1);
                break;
        }

        _lastCornerType = nextCornerType;
        return cornerObj.transform;
    }

    private Transform GenerateStraight(Transform lastTrack)
    {
        Transform endpoint = lastTrack.Find("EndPoint");
        GameObject straightObj = PoolManager.Instance.spawnObject(StraightPrefab, endpoint.position, endpoint.rotation);
        return straightObj.transform;
    }

    private Transform GenerateLevelFinish(Transform lastTrack) 
    {
        Transform endpoint = lastTrack.Find("EndPoint");
        GameObject finishObj = PoolManager.Instance.spawnObject(LevelFinishPrefab, endpoint.position, endpoint.rotation);
        return finishObj.transform;
    }

    // State machine for deciding next generate corner direction and type
    private CornerType DecideCorner()
    {
        CornerType result;

        if (_isLastGoingUp)
        {
            // !!! Must be opposite of last
            _isLastGoingUp = false;
            return Random.value < 0.5f ? CornerType.Right : CornerType.Left;
        }

        // set _isLastGoingUp true if next is not UCorner
        // last road was Going horizontal => can go opposite U or opposite side
        switch (_lastCornerType)
        {
            case CornerType.Left:
                if(Random.value < _UCornerChance)
                {
                    result = CornerType.URight;
                }
                else
                {
                    // set true for next decide
                    _isLastGoingUp = true;
                    result = CornerType.Right;
                }
                break;
            case CornerType.Right:
                if (Random.value < _UCornerChance)
                {
                    result = CornerType.ULeft;
                }
                else
                {
                    _isLastGoingUp = true;
                    result = CornerType.Left;
                }
                break;
            case CornerType.ULeft:
                _isLastGoingUp = true;
                result = CornerType.Right;
                break;
            case CornerType.URight:
                _isLastGoingUp = true;
                result = CornerType.Left;
                break;
            default:
                Debug.LogError("Decide Corner default hit!");
                result = CornerType.Left;
                break;
        }
        return result;
    }

    private void OnObjectSpawned(GameObject GO)
    {
        _trackReleaseQueue.Enqueue(GO);

        // Release old GameObjects of last level to be used for later
        // poolsize = object count need per level * generate next x level
        int poolSize = (_numberOfCornersPerLevel * 2 + 3) * 3;
        if (_trackReleaseQueue.Count == poolSize)
        {
            Debug.Log("Pool limit reached");
            GameObject obj = _trackReleaseQueue.Dequeue();
            PoolManager.Instance.releaseObject(obj);
        }
    }

}
