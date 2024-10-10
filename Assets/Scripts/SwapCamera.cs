using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapCamera : MonoBehaviour
{
    public CinemachineVirtualCamera leftCamera;
    public CinemachineVirtualCamera rightCamera;

    public float panDistance = 3f;
    public float panTime = 0.3f;

    
    private CinemachineVirtualCamera _currentCamera;
    private CinemachineFramingTransposer _framingTransposer;

    private void OnTriggerExit2D(Collider2D collision)
    {
        Vector2 exitDirection = (collision.transform.position - GetComponent<Collider2D>().bounds.center).normalized;

        if (collision.CompareTag("Player"))
        {
            SwapHorizontal(leftCamera, rightCamera, exitDirection);
        }
    }

    // Update is called once per frame
    public void SwapHorizontal(CinemachineVirtualCamera leftCamera, CinemachineVirtualCamera rightCamera, Vector2 triggerExitDirection)
    {
        if (_currentCamera == leftCamera && triggerExitDirection.x > 0f)
        {
            rightCamera.enabled = true;

            leftCamera.enabled = false;

            _currentCamera = rightCamera;

            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }
        else if (_currentCamera == rightCamera && triggerExitDirection.x < 0f)
        {
            leftCamera.enabled = true;

            rightCamera.enabled = false;

            _currentCamera = leftCamera;

            _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        }

    }
}
