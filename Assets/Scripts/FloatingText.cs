using UnityEngine;

// Script για floating UI text
public class FloatingText : MonoBehaviour
{
    // Ταχύτητα κίνησης προς τα πάνω
    public float moveSpeed = 50f;

    // Χρόνος ζωής
    public float lifeTime = 2f;

    RectTransform rectTransform;

    void Start()
    {
        // Παίρνουμε το RectTransform
        rectTransform = GetComponent<RectTransform>();

        // Καταστροφή μετά από λίγα δευτερόλεπτα
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Μετακίνηση προς τα πάνω
        rectTransform.anchoredPosition +=
            Vector2.up * moveSpeed * Time.deltaTime;
    }
}