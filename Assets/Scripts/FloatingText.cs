using UnityEngine;

// Script για floating UI text
public class FloatingText : MonoBehaviour
{
    // Ταχύτητα κίνησης προς τα πάνω
    public float moveSpeed = 50f;

    // Χρόνος ζωής του text πριν φύγει απο την οθόνη
    public float lifeTime = 2f;

    RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // Καταστροφή μετά από λίγα δευτερόλεπτα
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Μετακίνηση προς τα πάνω
        // anchoredPosition: Θέση του text στο Canvas 
        // Ανωδική κατεύθυνση (Vector2.up) 
        // Κίνηση ανεξάρτητη από το frame rate/ταχύτητα επεξεργασίας συσκευής (Time.deltaTime - default 1/60 sec)
        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;
    }
}