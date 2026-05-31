using UnityEngine;
using UnityEngine.XR.ARFoundation;

// Αναγνωριση και προσαρμογη φωτισμου με βαση τις συνθηκες του περιβαλλοντος
// Ενημέρωση του Directional Light της σκηνής ωστε τα 3D αντικείμενα να φωτίζονται ρεαλιστικά
[RequireComponent(typeof(Light))]
public class ARLightEstimation : MonoBehaviour
{
    // ARCameraManager για πρόσβαση στα δεδομένα φωτισμού από την κάμερα
    [SerializeField] private ARCameraManager cameraManager;

    // Ενημέρωση του Directional Light της σκηνής με βάση τις εκτιμήσεις φωτισμού
    private Light directionalLight;

    void Awake()
    {
        // Λήψη του Directional Light component από το GameObject
        directionalLight = GetComponent<Light>();
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    // Ενημέρωση του φωτισμού με βάση των δεδομένων του AR περιβάλλοντος
    // Για αποφυγή υπερβολικής φωτεινότητας ή σκοτεινότητας, εφαρμόζονται όρια έντασης (0.5f - 1.5f)
    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        // Ενημέρωση της έντασης του φωτισμού
        if (eventArgs.lightEstimation.averageBrightness.HasValue)
        {
            float brightness = eventArgs.lightEstimation.averageBrightness.Value;
            directionalLight.intensity = Mathf.Clamp(brightness, 0.5f, 1.5f); // min 0.5
        }

        // Ενημέρωση της θερμοκρασίας του φωτισμού
        if (eventArgs.lightEstimation.averageColorTemperature.HasValue)
            directionalLight.colorTemperature = eventArgs.lightEstimation.averageColorTemperature.Value;

        // Ενημέρωση του χρώματος του φωτισμού
        if (eventArgs.lightEstimation.colorCorrection.HasValue)
            directionalLight.color = eventArgs.lightEstimation.colorCorrection.Value;

        // Ενημέρωση της κατεύθυνσης του φωτισμού
        if (eventArgs.lightEstimation.mainLightDirection.HasValue)
            directionalLight.transform.rotation = Quaternion.LookRotation(
                eventArgs.lightEstimation.mainLightDirection.Value);
    }
}