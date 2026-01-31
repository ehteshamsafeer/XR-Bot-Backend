using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
public class SceneController : MonoBehaviour
{
    [SerializeField] InputActionReference _togglePlaneAction, _activateAction;
    [SerializeField] GameObject objects;
    bool isSpawned;

    private ARPlaneManager _planeManager;
    private bool _isVisible = true;
    private int _numPlanesAddedOccurred = 0;






    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isSpawned=false;
        _planeManager = GetComponent<ARPlaneManager>();
        if (_planeManager is null)
        {
            Debug.LogError("Can't Find Plane Manager");
        }

        _togglePlaneAction.action.performed += OnTogglePlaneAction;
        _planeManager.planesChanged += OnPlanesChanged;
        _activateAction.action.performed += Action_performed;
    }

    private void Action_performed(InputAction.CallbackContext obj)
    {
        Vector3 spawnPosition;
        foreach (var plane in _planeManager.trackables)
        {
            if (plane.classifications == UnityEngine.XR.ARSubsystems.PlaneClassifications.Table && isSpawned == false)
            {
                spawnPosition = plane.transform.position;
                spawnPosition.y += 0.2f;
                Instantiate(objects, spawnPosition, Quaternion.identity);
                isSpawned = true;
            }
        }
    }
    private void OnTogglePlaneAction(InputAction.CallbackContext obj)
    {
        _isVisible = !_isVisible;
        float fillAlpha = _isVisible ? 0.3f : 0f;
        float lineAlpha = _isVisible ? 1.0f : 0f;

        foreach (var plane in _planeManager.trackables)
        {
            SetPlaneAlpha(plane, fillAlpha, lineAlpha);
        }

    }

    private void SetPlaneAlpha(ARPlane plane, float fillAlpha, float lineAlpha)
    {
        var meshRenderer = plane.GetComponentInChildren<MeshRenderer>();
        var lineRenderer = plane.GetComponentInChildren<LineRenderer>();


        if (meshRenderer != null)
        {
            Color color = meshRenderer.material.color;
            color.a = fillAlpha;
            meshRenderer.material.color = color;
        }
        if (lineRenderer != null)
        {
            Color startColor = lineRenderer.startColor;
            Color endColor = lineRenderer.endColor;

            startColor.a = lineAlpha;
            endColor.a = lineAlpha;

            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;

        }
    }
    private void OnPlanesChanged (ARPlanesChangedEventArgs args)
    {
        if (args.added.Count > 0)
        {
            _numPlanesAddedOccurred++;
        }

    }
    void OnDestroy()
    {
        _togglePlaneAction.action.performed -= OnTogglePlaneAction;
        _planeManager.planesChanged -= OnPlanesChanged;
        _activateAction.action.performed -= Action_performed;
    }
}
