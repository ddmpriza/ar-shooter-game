using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Light))]               // Απαιτείται το Light component για να λειτουργήσει σωστά το script

// Αναγνωριση και προσαρμογη φωτισμου με βαση τις συνθηκες του περιβαλλοντος
// Ενημέρωση του Directional Light της σκηνής ωστε τα 3D αντικείμενα να φωτίζονται ρεαλιστικά
public class ARLightEstimation : MonoBehaviour
{
    // ARCameraManager για πρόσβαση στα δεδομένα φωτισμού πραγματικού περιβάλλοντος από την κάμερα
    [SerializeField] private ARCameraManager cameraManager;

    // Ενημέρωση του Directional Light της σκηνής με βάση τις εκτιμήσεις φωτισμού
    private Light directionalLight;

    void Awake()
    {
        // Λήψη του Directional Light component από το GameObject
        directionalLight = GetComponent<Light>();
    }

    // Κλήση με την έναρξη της σκηνής για να ξεκινήσει η παρακολούθηση των δεδομένων φωτισμού από την κάμερα
    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    // Ενημέρωση του φωτισμού με βάση των δεδομένων του AR περιβάλλοντος
    // eventArgs: πληροφορία που στέλνει το AR Foundation για κάθε frame και περιερχει δεδομένα φωτισμού
    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        // Αν υπάρχει τιμή και τα δεδομενα (eventArgs) δεν ειναι null
        if (eventArgs.lightEstimation.averageBrightness.HasValue)
        {
            // Λήψη της μέσης φωτεινότητας από τα δεδομένα φωτισμού 
            float brightness = eventArgs.lightEstimation.averageBrightness.Value;
            // Εφαρμογή της φωτεινότητας στην ένταση του Directional Light της σκηνής
            // Για αποφυγή υπερβολικής φωτεινότητας ή σκοτεινότητας, εφαρμόζονται όρια έντασης (0.5f - 1.5f)
            directionalLight.intensity = Mathf.Clamp(brightness, 0.5f, 1.5f); // min 0.5
        }

        // Ενημέρωση της θερμοκρασίας του φωτισμού
        if (eventArgs.lightEstimation.averageColorTemperature.HasValue)
            // Εφαρμογή της θερμοκρασίας χρώματος στο Directional Light της σκηνής
            directionalLight.colorTemperature = eventArgs.lightEstimation.averageColorTemperature.Value;

        // Ενημέρωση του χρώματος του φωτισμού
        if (eventArgs.lightEstimation.colorCorrection.HasValue)
            // Εφαρμογή της διόρθωσης χρώματος στο Directional Light της σκηνής
            directionalLight.color = eventArgs.lightEstimation.colorCorrection.Value;

        // Ενημέρωση της κατεύθυνσης του φωτισμού
        if (eventArgs.lightEstimation.mainLightDirection.HasValue)
            // Εφαρμογή της κατεύθυνσης του φωτισμού στο Directional Light της σκηνής
            // Quaternion.LookRotation: μετατροπή της κατεύθυνσης του φωτισμού σε περιστροφή (rotation) για το Directional Light
            directionalLight.transform.rotation = Quaternion.LookRotation(eventArgs.lightEstimation.mainLightDirection.Value);
    }
}