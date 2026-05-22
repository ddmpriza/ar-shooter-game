using UnityEngine;

public class ScaleObject : MonoBehaviour
{
    private GameObject selectedObject = null;
    private float scaleSpeed = 0.005f;
    private float minScale = 0.1f;
    private float maxScale = 3f;

    void Update()
    {
        if (GameStateManager.instance.gameStarted) return;
        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);

        // Tap για επιλογή
        if (touch.phase == TouchPhase.Began)
        {
            TrySelectObject(touch.position);
        }

        // Swipe για scale
        if (touch.phase == TouchPhase.Moved && selectedObject != null)
        {
            float delta = -touch.deltaPosition.y * scaleSpeed * Time.deltaTime * 60f;
            float currentScale = selectedObject.transform.localScale.x;
            float newScale = Mathf.Clamp(currentScale + delta, minScale, maxScale);
            selectedObject.transform.localScale = Vector3.one * newScale;
        }
    }

    private void TrySelectObject(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
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