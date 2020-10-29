using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RopeVisual : MonoBehaviour
{
    private CarMovement _carMovement;
    private LineRenderer _lineRenderer;
    // Start is called before the first frame update
    void OnEnable()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _carMovement = transform.parent.GetComponent<CarMovement>();
        _lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_carMovement.IsHooked)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, _carMovement.HookPoint);
        }
        else
        {
            _lineRenderer.enabled = false;
        }
    }
}
