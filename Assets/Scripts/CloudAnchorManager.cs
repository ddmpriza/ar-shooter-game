using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;

// Διαχειρστής για hosting και resolving cloud anchors
// Υλοποίσηση δύο λειτουργιών: 
// Hosting: Ο χρήστης τοποθετεί 2 anchors και τα ανεβάζει στο cloud
// Resolving: Οι χρήστες προσπαθούν να βρουν τα anchors που έχουν ανεβείpublic class CloudAnchorManager : MonoBehaviour
public class CloudAnchorManager : MonoBehaviour
{
    public static CloudAnchorManager instance;

    // Διαχείριση AR components
    [Header("AR Components")]
    // Αναφορά στον ARAnchorManager για hosting/resolving
    [SerializeField] private ARAnchorManager anchorManager;
    // Αναφορά στον ARRaycastManager για ανίχνευση planes και τοποθέτηση anchors
    [SerializeField] private ARRaycastManager raycastManager;

    // UI References
    [Header("UI")]
    // Panel που εμφανίζεται κατά τη διάρκεια του hosting με οδηγίες και κατάσταση
    [SerializeField] private GameObject hostingPanel;
    // Κείμενο που ενημερώνει τον χρήστη για την κατάσταση του hosting (π.χ. "Tap για anchor 1/2", "Uploading...", "✓ Ολοκληρώθηκε!")
    [SerializeField] private TMPro.TextMeshProUGUI hostingStatusText;
    // Κουμπί έναρξης παιχνιδιού που εμφανίζεται μετά το hosting
    [SerializeField] private GameObject startButton;
    // Κουμπί για shoot που εμφανίζεται μετά το hosting
    [SerializeField] private GameObject shootButton;
    // Κουμπί επανεκκίνησης που εμφανίζεται μετά το hosting
    [SerializeField] private GameObject retryButton;

    // Prefabs για τα anchors 
    [Header("Objects to spawn")]
    // Robot Kyle που εμφανίζεται στο πρώτο anchor
    [SerializeField] private GameObject anchor1Prefab;
    // Glossy object που εμφανίζεται στο δεύτερο anchor
    [SerializeField] private GameObject anchor2Prefab;

    // Λίστα αποθήκευσης αποτελεσμάτων raycast
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    // Πίνακας για αποθήκευση των IDs των cloud anchors που θα ανέβουν στο cloud
    private string[] cloudAnchorIds = new string[2];
    // Πίνακας για αποθήκευση των τοπικών ARAnchor references που δημιουργούνται κατά το hosting
    private ARAnchor[] localAnchors = new ARAnchor[2];


    // Μετρητής τοποθέτησης anchors 
    private int anchorCount = 0;

    // Flag σηματοδότησης hosting λειτουργίας
    // Αυτό θα χρησιμοποιηθεί από το PlaceObject για να απενεργοποιήσει την τοποθέτηση νέων objects κατά τη διάρκεια του hosting
    public bool isHosting = false;

    // Flag σηματοδότησης ολοκλήρωσης hosting
    // Αυτό θα χρησιμοποιηθεί για να ενεργοποιήσει την εμφάνιση των αντικειμένων στα σημεία των anchors μετά το hosting
    public bool hostingComplete = false;

    // Flag για να αποτρέψει πολλαπλά taps και hosting ταυτόχρονα
    private bool isWaitingForHosting = false;

    void Awake() => instance = this;

    // Κατά την εκκίνηση της σκηνής, ελέγχουμε αν το PlayerPref "StartHosting" είναι 1. 
    // Αν είναι, ξεκινάει η λειτουργία hosting. 
    // Αν δεν ειναι, γίνονται resolve anchors από προηγούμενη συνεδρία (αν υπάρχουν).
    void Start()
    {
        // Προτεραιότητα στο hosting αν το flag είναι ενεργοποιημένο
        if (PlayerPrefs.GetInt("StartHosting", 0) == 1)
        {
            // Reset του flag για να μην ξεκινάει συνέχεια hosting σε κάθε φόρτωση της σκηνής
            PlayerPrefs.SetInt("StartHosting", 0);
            PlayerPrefs.Save();
            StartHosting();
        }
        //  αλλιώς resolving
        else
        {
            StartCoroutine(DelayedResolve()); // Για μελλοντικές συνεδρίες
        }
    }

    private IEnumerator DelayedResolve()
    {
        yield return new WaitForSeconds(3f);
        ResolveAnchors();
    }

    // Ενεργοποίηση hosting λειτουργίας - καλείται από το MenuManager όταν ο χρήστης επιλέγει να ξεκινήσει 
    // με InitializeCloudAnchors (δηλαδή να τοποθετήσει τα cloud anchors)
    public void StartHosting()
    {
        isHosting = true;
        hostingComplete = false;
        anchorCount = 0;
        isWaitingForHosting = false;
        hostingPanel.SetActive(true);
        hostingStatusText.text = "Σκάναρε και tap για anchor 1/2";
        startButton.SetActive(false);
        shootButton.SetActive(false);
        retryButton.SetActive(false);
    }

    // Κάθε frame, το hosting mode ειναι ενεργό, ανιχνεύεται το tap του χρήστη, γίνεται raycast στο πλησιέστερο 
    // AR plane και δημιουργείται anchor εκεί.
    void Update()
    {
        // Έλεγχος Hosting mode και αποφυγή πολλαπλών taps
        if (!isHosting || isWaitingForHosting || anchorCount >= 2) return;
    
        // Αποφυγή τοποθέτησης νέων anchors αν έχουν ήδη τοποθετηθεί 2
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // Raycast για ανίχνευση επιπέδου και τοποθέτηση anchor
        if (!raycastManager.Raycast(touch.position, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon)) return;

        ARPlane plane = FindObjectOfType<ARPlaneManager>()?.GetPlane(hits[0].trackableId);
        if (plane == null) { Debug.LogError("Plane not found!"); return; }

        // Δημιουργία τοπικού anchor στο σημείο του tap
        ARAnchor anchor = anchorManager.AttachAnchor(plane, hits[0].pose);
        if (anchor == null) { Debug.LogError("Failed to create anchor!"); return; }

        // Αποθήκευση τοπικού reference για άμεσο spawn μετά το hosting
        localAnchors[anchorCount] = anchor;

        isWaitingForHosting = true;
        hostingStatusText.text = "Uploading anchor " + (anchorCount + 1) + "/2...";

        // Ανέβασμα του anchor στο cloud 
        StartCoroutine(WaitForHosting(anchorManager.HostCloudAnchorAsync(anchor, 1)));
    }

    // Aναμονή για το αποτέλεσμα του hosting και αποθήκευση των IDs των anchors
    private IEnumerator WaitForHosting(HostCloudAnchorPromise promise)
    {
        // Αναμονή για το αποτέλεσμα του hosting
        yield return promise;

        if (promise.Result.CloudAnchorState == CloudAnchorState.Success)
        {
            cloudAnchorIds[anchorCount] = promise.Result.CloudAnchorId;
            anchorCount++;
            Debug.Log("Anchor " + anchorCount + " hosted! ID: " + cloudAnchorIds[anchorCount - 1]);

            if (anchorCount < 2)
            {
                // Αν έχει τοποθετηθεί μόνο 1 anchor, ενημέρωση του χρήστη για το επόμενο anchor
                hostingStatusText.text = "Anchor 1/2  Tap για anchor 2/2";
                isWaitingForHosting = false;
            }
            else
            {
                // Αν έχουν τοποθετηθεί και τα 2 anchors, αποθήκευση των IDs για μελλοντική χρήση (resolving σε επόμενη συνεδρία)
                PlayerPrefs.SetString("CloudAnchorId1", cloudAnchorIds[0]);
                PlayerPrefs.SetString("CloudAnchorId2", cloudAnchorIds[1]);
                PlayerPrefs.Save();

                isHosting = false;
                hostingComplete = true;
                isWaitingForHosting = false;
                hostingStatusText.text = "Και τα 2 anchors τοποθετήθηκαν!";

                // Εμφάνιση κουμπιών παιχνιδιού
                hostingPanel.SetActive(false);
                startButton.SetActive(true);
                shootButton.SetActive(true);
                retryButton.SetActive(true);
                Debug.Log("Both anchors hosted!");

                // Spawn αμέσως στην ίδια συνεδρία χωρίς resolve
                SpawnAnchorObjects();
            }
        }
        else
        {
            Debug.LogError("Hosting failed: " + promise.Result.CloudAnchorState);
            hostingStatusText.text = "Απέτυχε! Δοκίμασε ξανά.";
            isWaitingForHosting = false;
        }
    }

    // Κλήση μετά το hosting — είτε αμέσως μετά το hosting της ίδιας συνεδρίας, 
    // είτε μετά το resolving σε επόμενη συνεδρία — 
    // για να εμφανίσει τα 3D αντικείμενα στις θέσεις των anchors
    private void SpawnAnchorObjects()
    {
        if (anchor1Prefab != null && localAnchors[0] != null)
            Instantiate(anchor1Prefab, localAnchors[0].transform.position,
                        localAnchors[0].transform.rotation);

        if (anchor2Prefab != null && localAnchors[1] != null)
            Instantiate(anchor2Prefab, localAnchors[1].transform.position,
                        localAnchors[1].transform.rotation);

        Debug.Log("Anchor objects spawned!");
    }

    // Φόρτωση των αποθηκευμένων cloud anchor IDs και προσπάθεια resolve.
    // Χρήση σε νέες συνεδρίες όπου δεν γίνεται hosting.
    public void ResolveAnchors()
    {
        // Ανάκτηση των αποθηκευμένων IDs των cloud anchors από τα PlayerPrefs
        string id1 = PlayerPrefs.GetString("CloudAnchorId1", "");
        string id2 = PlayerPrefs.GetString("CloudAnchorId2", "");

        // Αν δεν υπάρχουν αποθηκευμένα IDs, εμφάνιση μηνύματος και έξοδος
        if (id1 != "") StartCoroutine(WaitForResolving(
            anchorManager.ResolveCloudAnchorAsync(id1), 1));
        if (id2 != "") StartCoroutine(WaitForResolving(
            anchorManager.ResolveCloudAnchorAsync(id2), 2));
    }

    // Αναμονή αποτελέσματος του resolving και εμφάνιση των αντικειμένων στα σημεία των anchors αν το resolving ήταν επιτυχές
    private IEnumerator WaitForResolving(ResolveCloudAnchorPromise promise, int index)
    {
        while (promise.State == PromiseState.Pending)
            yield return null;

        yield return new WaitForSeconds(1f);

        CloudAnchorState state = CloudAnchorState.None;
        try { state = promise.Result.CloudAnchorState; } catch { }

        if (state == CloudAnchorState.Success)
        {
            try
            {
                Debug.Log("Anchor " + index + " resolved!");
                GameObject prefab = index == 1 ? anchor1Prefab : anchor2Prefab;
                Instantiate(prefab, promise.Result.Anchor.transform.position,
                                    promise.Result.Anchor.transform.rotation);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to instantiate anchor " + index + ": " + e.Message);
            }
        }
        else
            Debug.LogError("Resolving failed: " + state);
    }
}
