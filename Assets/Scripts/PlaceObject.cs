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
        // if (GameStateManager.instance.gameStarted) return;
        if (GameStateManager.instance != null && GameStateManager.instance.gameStarted) return;
        if (CloudAnchorManager.instance != null && 
            CloudAnchorManager.instance.isHosting) return;
        if (Input.touchCount == 0) return; 

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // Πρώτα έλεγχος αν το tap έγινε πάνω σε υπάρχον κυβάκι
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Target"))
            {
                // Tap σε κυβάκι - αποφυγή τοποθέτησης νέου object
                return;
            }
        }

        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            Vector3 spawnPos = hitPose.position + Vector3.up * 0.1f;
            Instantiate(objectToPlace, spawnPos, hitPose.rotation);
        }
    }
}