using UnityEngine;
public class HookPointDetector : MonoBehaviour
{
    public CornerOrientation Orientation { get; private set; }
    public Vector3? HookPoint => _lastHookPoint?.transform.position;

    [SerializeField, TagSelector]
    private string HOOK_POINT_TAG = "HookPoint";

    private GameObject _lastHookPoint = null;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(HOOK_POINT_TAG))
        {
            Orientation = other.transform.parent.localScale.x < 0 ? CornerOrientation.AntiClockwise : CornerOrientation.Clockwise;
            _lastHookPoint = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(HOOK_POINT_TAG))
        {
            return;
        }

        _lastHookPoint = null;

    }

}

public enum CornerOrientation
{
    AntiClockwise = -1,
    Clockwise = 1
}