using UnityEngine;

public class ScaleObject : MonoBehaviour
{
    private GameObject selectedObject = null;

    // Ταχύτητα αλλαγής μεγέθους του αντικειμένου
    private float scaleSpeed = 0.005f;
    // Ελάχιστο και μέγιστο μέγεθος του αντικειμένου
    private float minScale = 0.1f;
    private float maxScale = 3f;

    void Update()
    {
        // Μόνο όταν το game ΔΕΝ έχει ξεκινήσει μπορούμε να αλλάζουμε το μέγεθος των objects
        if (GameStateManager.instance.gameStarted) return;
        // Μόνο όταν υπάρχει tap αλλάζει το μέγεθος του αντικειμένου
        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);

        // Επιλογή αντικειμένου, μόνο οταν γίνεται Tap
        if (touch.phase == TouchPhase.Began)
        {
            TrySelectObject(touch.position);
        }

        // Swipe για scale
        if (touch.phase == TouchPhase.Moved && selectedObject != null)
        {
            // Μέγεθος/Ένταση κίνησης του swipe για αλλαγή μεγέθους (touch.deltaPosition.y)
            float delta = -touch.deltaPosition.y * scaleSpeed * Time.deltaTime * 60f;

            float currentScale = selectedObject.transform.localScale.x;
            // Διατήρηση του νέου μεγέθους εντός των ορίων minScale και maxScale
            float newScale = Mathf.Clamp(currentScale + delta, minScale, maxScale);
            // ομοιόμορφη αλλαγή μεγέθους σε όλους τους άξονες (x, y, z) (Vector3.one * newScale)
            selectedObject.transform.localScale = Vector3.one * newScale;
        }
    }

    // Επιλογή αντικειμένου που επιλέχθηκε
    private void TrySelectObject(Vector2 screenPos)
    {
        // Μετατροπή του σημείου της οθόνης (screenPos) σε ακτίνα (Ray) που ξεκινάει από την κάμερα και κατευθύνεται
        // προς το σημείο που έγινε tap
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        
        if (Physics.Raycast(ray, out hit))
        {
            // Αν το αντικείμενο που χτυπήθηκε είναι κυβάκι (tag "Target"), τότε επιλέγεται και αποθηκεύεται στο selectedObject
            if (hit.collider.gameObject.CompareTag("Target"))
            {
                selectedObject = hit.collider.gameObject;
                Debug.Log("Selected: " + selectedObject.name);
            }
            else
            {
                selectedObject = null;
            }
        }
        else
        {
            selectedObject = null;
        }
    }
}