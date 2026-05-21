using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObject : MonoBehaviour
{
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject objectToPlace; // το 3D object σου

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool canPlace = false;

    public void EnablePlacement() => canPlace = true;
    public void DisablePlacement() => canPlace = false;

    void Update()
    {
        // Μόνο όταν το game ΔΕΝ έχει ξεκινήσει μπορούμε να τοποθετούμε objects
        if (GameStateManager.instance.gameStarted) return;
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            Instantiate(objectToPlace, hitPose.position, hitPose.rotation);
        }
    }
}