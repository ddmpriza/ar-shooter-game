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
    [Header("AR Components")]
    [SerializeField] private AREarthManager earthManager;
    [SerializeField] private ARAnchorManager anchorManager; // ΝΕΟΣ
    

    [Header("Target Location - Εθν. Αμύνης & Αγ. Δημητρίου")]
    private const double TargetLatitude  = 40.6308107;// 3D μοντέλο που θα εμφανιστεί;
    private const double TargetLongitude = 22.9517927;
    // Ύψος σε μέτρα (0 = επίπεδο εδάφους)
    private const double TargetAltitude  = 0.0; 
    // Αποδεκτή απόσταση σε μέτρα για τοποθέτηση του μοντέλου (π.χ. 30μ)
    private const double PlacementRadius = 100.0;  

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
        while (earthManager == null || 
            earthManager.EarthTrackingState != TrackingState.Tracking)
        {
            if (earthManager != null)
                Debug.Log("EarthTrackingState: " + earthManager.EarthTrackingState);
            else
                Debug.LogError("earthManager is NULL!");
            yield return new WaitForSeconds(1f);
        }

        isTracking = true;
        statusText.text = "GPS ενεργό!";
    }

    void Update()
    {
        Debug.Log("Geospetial Update - isTracking: " + isTracking + " modelSpawned: " + modelSpawned);

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
        if (distance <= PlacementRadius)
        {
            GeospatialPose p = earthManager.CameraGeospatialPose;
            if (p.HeadingAccuracy > 25 || p.HorizontalAccuracy > 15)
            {
                statusText.text = string.Format(
                    "GPS ακρίβεια χαμηλή ({0:F0}°)\nΠεριμένετε...", p.HeadingAccuracy);
                return;
            }
            PlaceModelAtTarget(geospatialPose.Altitude);
        }        
        }
        else
        {
            Debug.Log("Geospetial Εκτός radius. Απόσταση: " + distance + " Radius: " + PlacementRadius);
        }
        
    }


    // Τοποθέτηση του Kyle στο σημείο στόχο με χρήση του Geospatial API
    private void PlaceModelAtTarget(double currentAltitude)
    {
        // Σταματάμε αμέσως τις επαναλήψεις
        modelSpawned = true;

        if (modelPrefab == null)
        {
            Debug.LogError("Model Prefab δεν έχει οριστεί!");
            return;
        }

        // Έλεγχος ακρίβειας GPS πριν δημιουργια του anchor
        GeospatialPose pose = earthManager.CameraGeospatialPose;
        Debug.Log("Heading accuracy: " + pose.HeadingAccuracy + 
                " Position accuracy: " + pose.HorizontalAccuracy);

        double altitude = currentAltitude - 1.5;

        try
        {
            ARGeospatialAnchor anchor = ARAnchorManagerExtensions.AddAnchor(
                anchorManager,  // αντί για FindObjectOfType<ARAnchorManager>()
                TargetLatitude, TargetLongitude, altitude, Quaternion.identity);

            if (anchor != null)
            {
                Instantiate(modelPrefab, anchor.transform);
                statusText.text = "✓ Μοντέλο τοποθετήθηκε!";
                Debug.Log("Geospatial model placed!");
            }
            else
            {
                modelSpawned = false;
                Debug.LogError("AddAnchor returned null!");
                statusText.text = "Σφάλμα. Δοκίμασε ξανά...";
            }
        }
        catch (System.Exception e)
        {
            modelSpawned = false;
            Debug.LogError("AddAnchor exception: " + e.Message);
            statusText.text = "Exception: " + e.Message;
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
        UnityEngine.SceneManagement.SceneManager.LoadScene("MyARGame");
    }
}
