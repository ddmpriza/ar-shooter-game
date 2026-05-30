using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Διαχειρίζεται το Geospatial API του ARCore.
/// Τοποθετεί ένα 3D μοντέλο στη γωνία Εθν. Αμύνης & Αγ. Δημητρίου, Θεσσαλονίκη.
/// </summary>
// Διαχειριση της λειτουργίας Geospatial API για συγκεκριμένο σημείο στη Θεσσαλονίκη
// Τοποθέτηση του Kyle στο σημείο όταν ο χρήστης πλησιάσει αρκετά κοντά
public class GeospatialManager : MonoBehaviour
{
    [Header("AR Components")]
    // Geospatial API απαιτεί ARSession και AREarthManager
    [SerializeField] private AREarthManager earthManager;        

    [Header("Target Location - Εθν. Αμύνης & Αγ. Δημητρίου")]
    private const double TargetLatitude  = 40.63066;
    private const double TargetLongitude = 22.95192;
    // Ύψος σε μέτρα (0 = επίπεδο εδάφους)
    private const double TargetAltitude  = 0.0; 
    // Αποδεκτή απόσταση σε μέτρα για τοποθέτηση του μοντέλου (π.χ. 30μ)
    private const double PlacementRadius = 30.0;  

    [Header("Object to Spawn")]
    // ο Kyle που θα εμφανιστεί όταν ο χρήστης πλησιάσει αρκετά κοντά στο στόχο
    [SerializeField] private GameObject modelPrefab;              

    [Header("UI")]
    [SerializeField] private TMPro.TextMeshProUGUI statusText;    
    // Κουμπί επιστροφής στο μενού
    [SerializeField] private GameObject backButton;               

    // Έλεγχος αν έχει ήδη τοποθετηθεί το μοντέλο 
    private bool modelSpawned = false;   
    // Έλεγχος αν το Geospatial API κάνει tracking
    private bool isTracking = false;     

    void Start()
    {
        modelSpawned = false;
        isTracking = false;
        statusText.text = "Αρχικοποίηση Geospatial API...";
        StartCoroutine(WaitForEarthTracking());
    }


    // Αναμονή για το EarthManager να κάνει tracking πριν ξεκινήσει ο έλεγχος θέσης
    private IEnumerator WaitForEarthTracking()
    {
        // Αναμονή μέχρι το ARCore να αρχικοποιηθεί
        yield return new WaitUntil(() =>
            earthManager != null &&
            earthManager.EarthTrackingState == TrackingState.Tracking);

        isTracking = true;
        statusText.text = "GPS ενεργό! Πήγαινε στη γωνία\nΕθν. Αμύνης & Αγ. Δημητρίου";
    }

    void Update()
    {
        if (!isTracking || modelSpawned) return;

        // Έλεγχος αν το EarthManager κάνει tracking
        if (earthManager.EarthTrackingState != TrackingState.Tracking) return;

        GeospatialPose geospatialPose = earthManager.CameraGeospatialPose;

        // Υπολογισμός απόστασης από τον στόχο
        double distance = GetDistance(
            geospatialPose.Latitude, geospatialPose.Longitude,
            TargetLatitude, TargetLongitude);

        // Ενημέρωση το UI με την απόσταση
        statusText.text = string.Format(
            "Απόσταση από στόχο: {0:F0}μ.\nΕθν. Αμύνης & Αγ. Δημητρίου",
            distance);

        //  Τοποθέτηση του μοντέλου αν η απόσταση είναι μικρότερη ή ίση με το PlacementRadius
        if (distance <= PlacementRadius)
        {
            PlaceModelAtTarget(geospatialPose.Altitude);
        }
    }


    // Τοποθέτηση του Kyle στο σημείο στόχο με χρήση του Geospatial API
    private void PlaceModelAtTarget(double currentAltitude)
    {
        if (modelPrefab == null)
        {
            Debug.LogError("Model Prefab δεν έχει οριστεί!");
            return;
        }

        // Χρήση το τρέχον altitude της κάμερας για το επίπεδο εδάφους
        double altitude = currentAltitude - 1.5; // -1.5μ για να είναι στο έδαφος

        // Δημιουργία geospatial anchor στο σημείο στόχο
        ARGeospatialAnchor anchor = ARAnchorManagerExtensions.AddAnchor(
            FindObjectOfType<ARAnchorManager>(),
            TargetLatitude,
            TargetLongitude,
            altitude,
            Quaternion.identity);

        if (anchor != null)
        {
            // Εμφανίζεται το μοντέλο πάνω στο anchor
            Instantiate(modelPrefab, anchor.transform);
            modelSpawned = true;
            statusText.text = "Μοντέλο τοποθετήθηκε!";
            Debug.Log("Geospatial model placed at target!");
        }
        else
        {
            Debug.LogError("Failed to create geospatial anchor!");
            statusText.text = "Σφάλμα τοποθέτησης. Δοκίμασε ξανά.";
        }
    }

    // Υπολογισμός απόστασης μεταξύ δύο GPS συντεταγμένων (Haversine formula)
    private double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Ακτίνα Γης σε μέτρα
        double dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        double dLon = (lon2 - lon1) * Mathf.Deg2Rad;
        double a = Mathf.Sin((float)(dLat / 2)) * Mathf.Sin((float)(dLat / 2)) +
                   Mathf.Cos((float)(lat1 * Mathf.Deg2Rad)) *
                   Mathf.Cos((float)(lat2 * Mathf.Deg2Rad)) *
                   Mathf.Sin((float)(dLon / 2)) * Mathf.Sin((float)(dLon / 2));
        double c = 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1 - a)));
        return R * c;
    }


    // Επιστροφή στο κύριο μενού με το πάτημα του Back button
    public void OnBackButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
}
