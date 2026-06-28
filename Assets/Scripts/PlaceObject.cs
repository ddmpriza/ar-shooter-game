using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceObject : MonoBehaviour
{
    // Άνοιγμα ενός attribute για να εμφανιστεί στο inspector του Unity Editor
    // RaycasrManager: λήψη του raycast hits από το AR Foundation
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject objectToPlace; // το 3D object

    // λίστα για διατήρηση των hits από το raycast
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        // Μόνο όταν το game ΔΕΝ έχει ξεκινήσει μπορούμε να τοποθετούμε objects
        if (GameStateManager.instance != null && GameStateManager.instance.gameStarted) return;

        // Μόνο όταν ΔΕΝ τοποθετούμε anchors μπορούμε να τοποθετούμε objects
        if (CloudAnchorManager.instance != null && CloudAnchorManager.instance.isHosting) return;

        if (Input.touchCount == 0) return; 

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // Πρώτα έλεγχος αν το tap έγινε πάνω σε υπάρχον κυβάκι
        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        RaycastHit hit;

        // Physics.Raycast για να ελέγξουμε αν το tap έγινε πάνω σε ένα αντικείμενο με tag "Target" 
        // Αναγνώριση του αντικειμένου Unity, όχι επιφάνειες (AR)
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Target"))
            {
                // Tap σε κυβάκι - αποφυγή τοποθέτησης νέου object
                return;
            }
        }

        // Εύρεση επιπέδου AR plane και τοποθέτηση του αντικειμένου
        // Αν η ακτίνα χτύπησε AR plane (raycastManager.Raycast) επιστρέφονται 3 τιμές
        // Το σημείο της οθόνης που έγινε tap (touch.position)
        // η λίστα που θα γεμίσει με τα hits αν βρεθεί επιφάνεια (hits)
        // φίλτρο που διασφαλίζει ότι το hit έγινε σε AR plane (TrackableType.PlaneWithinPolygon)
        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            // Λήψη της πρώτης θέσης από τα hits/ πιο κοντινή θέση + κλίση του επιπέδου
            Pose hitPose = hits[0].pose;
            // Τοποθέτηση αντικειμένου 
            Vector3 spawnPos = hitPose.position + Vector3.up * 0.1f;
            // Δημιουργία αντίγραφου του αντικειμένου στην επιλεγμένη θέση με την ίδια κλίση του hitPose
            Instantiate(objectToPlace, spawnPos, hitPose.rotation);
        }
    }
}