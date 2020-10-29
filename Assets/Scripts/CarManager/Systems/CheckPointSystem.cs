using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointReachedEventArgs : EventArgs
{
    public int CheckPointNumber { get; private set; }

    public CheckPointReachedEventArgs(int checkPointNumber)
    {
        CheckPointNumber = checkPointNumber;
    }
}

public class CheckPointSystem : MonoBehaviour
{
    public event Action<CheckPointReachedEventArgs> CheckPointReachedEvent;

    [SerializeField, TagSelector]
    private string CHECK_POINT_TAG = "CheckPoint";

    private int _checkPointProgressCounter = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(CHECK_POINT_TAG))
        {
            _checkPointProgressCounter++;
            CheckPointReachedEvent?.Invoke(new CheckPointReachedEventArgs(_checkPointProgressCounter));
        }
    }

}
