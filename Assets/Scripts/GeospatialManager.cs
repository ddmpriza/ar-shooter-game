using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARSubsystems;

// Διαχειρίζεται το Geospatial API του ARCore.
// Τοποθετεί ένα 3D μοντέλο στη γωνία Εθν. Αμύνης και Αγ. Δημητρίου, Θεσσαλονίκη.

// Διαχειριση της λειτουργίας Geospatial API για συγκεκριμένο σημείο στη Θεσσαλονίκη
// Τοποθέτηση του Kyle στο σημείο όταν ο χρήστης πλησιάσει αρκετά κοντά
public class GeospatialManager : MonoBehaviour
{
    [Header("AR Components")]                               // Οργάνωση του Inspector με [He
    //AREarthManager: Βιβλιοθήκη που παρέχει πρόσβαση στο Geospatial API (συντεταγμένες)
    [SerializeField] private AREarthManager earthManager;
    // ARAnchorManager: Βιβλιοθήκη για την δημιουργία Geospatial Anchor σε συγκεκριμένες συντεταγμένες
    [SerializeField] private ARAnchorManager anchorManager; 
    

    [Header("Target Location - Εθν. Αμύνης & Αγ. Δημητρίου")]
    private const double TargetLatitude  = 40.6308107;// 3D μοντέλο που θα εμφανιστεί;
    private const double TargetLongitude = 22.9517927;
    // Ύψος σε μέτρα (0 = επίπεδο εδάφους)
    private const double TargetAltitude  = 0.0; 
    // Αποδεκτή απόσταση σε μέτρα για τοποθέτηση του μοντέλου (π.χ. 100μ)
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
        // Ξεκινά την συνάρτηση GPS tracking
        StartCoroutine(WaitForEarthTracking());
    }


    // Αναμονή για το EarthManager να κάνει tracking πριν ξεκινήσει ο έλεγχος θέσης
    private IEnumerator WaitForEarthTracking()
    {
        // κάθε 1" ελέγχει αν το Geospatial API είναι έτοιμο για εντοπισμό της θέσης του χρήστη
        while (earthManager == null || earthManager.EarthTrackingState != TrackingState.Tracking)
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
        // Αν το μοντέλο είναι ήδη τοποθετημένο δεν συνεχίζει
        if (!isTracking || modelSpawned) return;

        // Έλεγχος αν το EarthManager κάνει tracking, αν χάθηκε το GPS σήμα σταματάει
        if (earthManager.EarthTrackingState != TrackingState.Tracking) return;

        // Λήψη της τρέχουσας θέσης της κάμερας
        GeospatialPose geospatialPose = earthManager.CameraGeospatialPose;

        // Υπολογισμός απόστασης της τρέχουσας θέσης από τον στόχο
        double distance = GetDistance(
            geospatialPose.Latitude, geospatialPose.Longitude,
            TargetLatitude, TargetLongitude);

        // Ενημέρωση το UI με την απόσταση
        statusText.text = string.Format("Απόσταση από στόχο: {0:F0}μ.\nΕθν. Αμύνης & Αγ. Δημητρίου", distance);

        //  Τοποθέτηση του μοντέλου αν η απόσταση είναι μικρότερη ή ίση με το PlacementRadius
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
        else
        {
            Debug.Log("Geospetial Εκτός radius. Απόσταση: " + distance + " Radius: " + PlacementRadius);
        }
        
    }


    // Τοποθέτηση του Kyle στο σημείο στόχο με χρήση του Geospatial API
    private void PlaceModelAtTarget(double currentAltitude)
    {
        // ΔΙακοπή επαναλήψεων - Δεν ξανακαλείται η Update()
        modelSpawned = true;

        if (modelPrefab == null)
        {
            Debug.LogError("Model Prefab δεν έχει οριστεί!");
            return;
        }

        // Έλεγχος ακρίβειας GPS πριν δημιουργια του anchor
        GeospatialPose pose = earthManager.CameraGeospatialPose;
        
        // Υπολογίζει το anchor περίπου 1.5 μέτρο κάτω από την κάμερα - Δεδομένου ότι η κάμερα δεν βρίσκεται στο σημείο 0
        double altitude = currentAltitude - 1.5;

        try
        {
            // AddAnchor: Επιτρέπει την δημιουργία Geospatial ANchor στις GPS συντεταγμένες χώρου
            ARGeospatialAnchor anchor = ARAnchorManagerExtensions.AddAnchor(
                anchorManager,  
                TargetLatitude, TargetLongitude, altitude, Quaternion.identity);

            if (anchor != null)
            {
                // Αρχιοθέτηση του Anchor με συντεταγμένες αμεσα εξαρτημένες από GPS του AR
                //  anchor.transform: αυτόματη ενημέρωση και διόρθωση της GPS θεσης
                Instantiate(modelPrefab, anchor.transform);
                statusText.text = "Μοντέλο τοποθετήθηκε!";
                Debug.Log("Geospatial model placed!");
            }
            else
            {   
                // αν για κάποιο λόγο η τοποθέτηση του anchor δεν πραγματοποιηθεί επιτυχώς επαναλαμβάνεται η συνάρτηση Update
                modelSpawned = false;
                Debug.LogError("AddAnchor returned null!");
                statusText.text = "Σφάλμα. Δοκίμασε ξανά...";
            }
        }
        catch (System.Exception e)
        {
            // αν αποτύχει η δημιουργία του μοντέλου, επαναλαμβάνεται η διαδικασία τοποθέτησης (επιστροφή στην Update)
            modelSpawned = false;
            Debug.LogError("AddAnchor exception: " + e.Message);
            statusText.text = "Exception: " + e.Message;
        }
    }

    // Υπολογισμός απόστασης μεταξύ δύο GPS συντεταγμένων (Haversine formula)
    // lat1: τρέχον γεωγραφικό πλάτος του χρήστη
    // lon1: τρέχον γεωγραφικό μήκος του χρήστη
    // lat2: τρέχον γεωγραφικό πλάτος του στόχου
    // lon2: τρέχον γεωγραφικό μήκος του ατόχου
    private double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000;                           // Ακτίνα Γης σε μέτρα
        double dLat = (lat2 - lat1) * Mathf.Deg2Rad;        // Μετατροπή της διαφοράς γεωγραφικού πλάτους από μοίρες σε ακτίνια
        double dLon = (lon2 - lon1) * Mathf.Deg2Rad;        // Μετατροπή της διαφοράς γεωγραφικού μήκους από μοίρες σε ακτίνια

        // Haversine formula - Απόσταση μεταξύ θέση χρήστη και θέση στόχου
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
