using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[SerializeField]
	private float _pitchOffset = 0.0f;
	[SerializeField]
	private float _smoothTime = 0.125f;
	[SerializeField]
	private Vector3 _offset;
	[SerializeField]
	private Transform _target;
	
	private Vector3 _currentVelocity;

	public void Init(Transform target)
    {
		_target = target;
		Vector3 desiredPosition = _target.position + _offset;
		transform.position = desiredPosition;
		transform.LookAt(_target);
	}

	private void LateUpdate()
	{
		Vector3 desiredPosition = _target.position + _offset;
		Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, _smoothTime);

		transform.position = new Vector3(smoothedPosition.x, _offset.y, smoothedPosition.z);
		transform.LookAt(_target);
		if(_pitchOffset != 0.0f)
        {
			transform.Rotate(Vector3.right, _pitchOffset);
		}
	}
}
