using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;                         // Geospatial API της Google
using UnityEngine.XR.ARSubsystems;                        // TrackingState enumeration

public class GeospatialManager : MonoBehaviour
{
    [SerializeField] private AREarthManager earthManager;           // GPS θεση μέσω Geospatial API
    [SerializeField] private ARAnchorManager anchorManager;         // Δημιουργία anchor
    [SerializeField] private GameObject modelPrefab;                // O Kyle!
    [SerializeField] private TMPro.TextMeshProUGUI statusText;      // Κείμενο ενημέρωσης κατάστασης 
    [SerializeField] private GameObject backButton;                 // Κουμπί για πίσω στο menu
 
    // Ορισμός τοποθεσίας που θα εισαχηθεί ο Anchor
    private const double TargetLatitude  = 40.63062865923928;
    private const double TargetLongitude = 22.951892358166443;
    // Από τι απόσταση και μετά θα εμφανιστεί ο Anchor
    private const double PlacementRadius = 100.0;

    private bool modelSpawned = false;
    private bool isTracking = false;

    // Αρχικοποίηση μεταβλητών και ξεκινά το Coroutine Geospatial API
    void Start()
    {
        modelSpawned = false;
        isTracking = false;
        statusText.text = "Αρχικοποίηση Geospatial API...";
        StartCoroutine(WaitForEarthTracking());
    }

    // Αναμονή του Geospatial API για tracking - λήψη GPS σήματος
    private IEnumerator WaitForEarthTracking()
    {
        // έλεγχος σύνδεσης του component στον Inspector (earthManager)
        // και οτι το ARCore έχει εντοπίσει την θέση του χρήστη
        while (earthManager == null || earthManager.EarthTrackingState != TrackingState.Tracking)
        {
            yield return null;
        }

        isTracking = true;
        statusText.text = "GPS ενεργό!";
    }

    void Update()
    {
        // Αν το GPS ακόμα αναζητά την θέση του χρήστη ή το μοντέλο τοποθετήθηκε - έξοδος
        if (!isTracking || modelSpawned) return;

        // Αν χαθεί το GPS σήμα κατά την διάρκεια του παιχνιδιού - έξοδος 
        if (earthManager.EarthTrackingState != TrackingState.Tracking) return;

        // Λήψη της τρέχουσας τοποθεσίας της κάμερας (Πλάτος (Lat), Μήκος (Long), Υψόμετρο (Alt), Ακρίβεια)
        GeospatialPose pose = earthManager.CameraGeospatialPose;

        // Κλήση συνάρτησης GetDistance - υπολογισμός απόστασης μεταξύ τρέχουσας θέσης και στόχου
        // Pose: Η θέση του χρήστη
        // Target: Θέση στόχου
        double distance = GetDistance(
            pose.Latitude, pose.Longitude,
            TargetLatitude, TargetLongitude);

        // Εμφάνιση στην οθόνη την απόσταση χρήστη - στόχου
        statusText.text = string.Format(
            "Απόσταση: {0:F0}μ.\nΕγνατία & Δημ.Γούναρη", distance);

        // Έλεγχος αν ο χρήστης είναι εντός των επιτρεπόμενων ορίων για εμφανιση του στόχου
        if (distance <= PlacementRadius)
        {
            // Έλεγχος της ακρίβειας του GPS πριν την εμφάνιση του anchor
            if (pose.HeadingAccuracy > 25 || pose.HorizontalAccuracy > 15)
            {
                statusText.text = string.Format(
                    "GPS χαμηλή ακρίβεια ({0:F0}°)\nΠεριμένετε...", pose.HeadingAccuracy);
                return;
            }
            // Αν ξεπεραστεί το σφάλμα - εμφάνιση Kyle!
            PlaceModelAtTarget(pose.Altitude);
        }
    }

    private void PlaceModelAtTarget(double currentAltitude)
    {
        // Θεωρεί το μοντέλο τοπθετημένο - Η update δεν ξανα καλεί την PlaceModelAtTarget
        modelSpawned = true;
        // Αφαίρεση μισόυ μέτρου από το υψόμετρο της κάμερας - ο Kyle να βρίσκεται στο πάτωμα όχι στο ύψος των ματιων του χρήστη
        double altitude = currentAltitude - 1.5;

        try
        {
            // Δημιουργείται μέσω του ARCore ένα Geospatial Anchor στις συντεταγμένες του στόχου
            ARGeospatialAnchor anchor = ARAnchorManagerExtensions.AddAnchor(
                anchorManager,
                TargetLatitude, TargetLongitude, altitude, Quaternion.identity);

            // Αν επέστρεψε/δημιουργήθηκε Anchor - ο Kyle τοποθετείται στις συντεταγμένες που ορίστηκαν
            if (anchor != null)
            {
                // anchor.transform: Ο Kyle ταυτίζεται με την θέση του anchor - αν υπάρξει διόρθωση των συντεταγμένων
                // από το AR Core ο Kyle θα μετακινηθεί ανάλογα
                Instantiate(modelPrefab, anchor.transform);
                statusText.text = "Μοντέλο τοποθετήθηκε! (Geospatial)";
            }
            else
            {
                // Αν η συσκευή δεν υποστηρίζει Geospatial Anchors ο Kyle τοποθετείται ως local anchor
                // Fallback: τοποθέτηση του Kyle 2 μέτρα μπροστά από κάμερα
                Vector3 spawnPos = Camera.main.transform.position + 
                                Camera.main.transform.forward * 2f;
                Instantiate(modelPrefab, spawnPos, Quaternion.identity);
                statusText.text = "Μοντέλο τοποθετήθηκε! (Local)";
            }
        }
        catch (System.Exception)
        {
            // Fallback: τοποθέτηση μπροστά από κάμερα
            Vector3 spawnPos = Camera.main.transform.position + 
                            Camera.main.transform.forward * 2f;
            Instantiate(modelPrefab, spawnPos, Quaternion.identity);
            statusText.text = "Μοντέλο τοποθετήθηκε! (Local)";
        }
    }

    private double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;                           // Ακτίνα Γης σε μέτρα
        double dLat = (lat2 - lat1) * Mathf.Deg2Rad;        // Μετατροπή της διαφοράς γεωγραφικού πλάτους από μοίρες σε ακτίνια
        double dLon = (lon2 - lon1) * Mathf.Deg2Rad;        // Μετατροπή της διαφοράς γεωγραφικού μήκους από μοίρες σε ακτίνια
        // Haversine formula - Απόσταση μεταξύ θέση χρήστη και θέση στόχου
        double a = Mathf.Sin((float)(dLat/2)) * Mathf.Sin((float)(dLat/2)) +
                   Mathf.Cos((float)(lat1 * Mathf.Deg2Rad)) *
                   Mathf.Cos((float)(lat2 * Mathf.Deg2Rad)) *
                   Mathf.Sin((float)(dLon/2)) * Mathf.Sin((float)(dLon/2));
        return R * 2 * Mathf.Atan2(Mathf.Sqrt((float)a), Mathf.Sqrt((float)(1-a)));
    }

    public void OnBackButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MyARGame");
    }
}