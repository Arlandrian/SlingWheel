using System;
using UnityEngine;

public class FailDetector : MonoBehaviour
{
    public event Action FailEvent;

    [SerializeField, TagSelector]
    private string WALL_TAG = "Wall";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(WALL_TAG))
        {
            FailEvent?.Invoke();
        }
    }
}
