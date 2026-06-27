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
    [Header("AR Components")]                                       // Οργάνωση του Inspector με [Header]
    // ARAnchorManager: Διαχείριση των anchors - βιβλιοθήκη για hosting και resolving cloud anchors
    [SerializeField] private ARAnchorManager anchorManager;
    // ARRaycastManager: Ανίχνευση planes και τοποθέτηση anchors - βιβλιοθήκη για raycasting και ανίχνευση επιπέδων
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

    // Flag σηματοδότησης hosting λειτουργίας - λειτουργίας τοποθέτησης anchors
    // Αυτό θα χρησιμοποιηθεί από το PlaceObject για να απενεργοποιήσει την τοποθέτηση νέων objects κατά τη διάρκεια του hosting
    public bool isHosting = false;

    // Flag σηματοδότησης ολοκλήρωσης hosting
    // Αυτό θα χρησιμοποιηθεί για να ενεργοποιήσει την εμφάνιση των αντικειμένων στα σημεία των anchors μετά το hosting
    public bool hostingComplete = false;

    // Flag για να αποτρέψει πολλαπλά taps και hosting ταυτόχρονα
    private bool isWaitingForHosting = false;

    // Singleton instance για εύκολη πρόσβαση από άλλα scripts
    void Awake()
    { 
        instance = this; 
    }

    void Start()
    {
        // Κατά την εκκίνηση της σκηνής, ελέγχουμε αν το PlayerPref "StartHosting" είναι 1
        // Αν είναι, ξεκινάει η λειτουργία hosting
        if (PlayerPrefs.GetInt("StartHosting", 0) == 1)
        {
            // Reset του flag για να μην ξεκινάει συνέχεια hosting σε κάθε φόρτωση της σκηνής
            PlayerPrefs.SetInt("StartHosting", 0);
            PlayerPrefs.Save();
            StartHosting();
        }
        // Αν δεν ειναι, γίνονται resolve anchors από προηγούμενη συνεδρία (αν υπάρχουν) και ξεκινάει GameMode
        else
        {
            // Χρήση StartCoroutine για εκτέλεση IEnumerator
            // Και χρήση IEnumerator για εκτέλεση yield
            StartCoroutine(DelayedResolve()); // Για μελλοντικές συνεδρίες
        }
    }

    // IEnumerator: Εμφάνιση των τιμών σταδιακά
    private IEnumerator DelayedResolve()
    {
        // Αναμονή 3" - Αναμονή Αρχικοποίησης του AR Session
        yield return new WaitForSeconds(3f);

        // Αν δεν υπάρχουν αποθηκευμένες θέσεις τοτε σταματάει η αναζητηση anchors
        // PlayerPrefs: Μόνιμη αρχειοθέτηση θέσεων στην συσκευή
        if (!PlayerPrefs.HasKey("A1x")) yield break;

        // Ανάκτηση των αποθηκευμένων θέσεων και κλίσεων των δύο anchors
        Vector3 pos1 = new Vector3(
            PlayerPrefs.GetFloat("A1x"),
            PlayerPrefs.GetFloat("A1y"),
            PlayerPrefs.GetFloat("A1z"),
            PlayerPrefs.GetFloat("A1rw"));

        Vector3 pos2 = new Vector3(
            PlayerPrefs.GetFloat("A2x"),
            PlayerPrefs.GetFloat("A2y"),
            PlayerPrefs.GetFloat("A2z"),
            PlayerPrefs.GetFloat("A2rw"));

        // Εμφάνιση των anchors στην αποθηκευμένη θέση
        Instantiate(anchor1Prefab, pos1, Quaternion.identity);
        Instantiate(anchor2Prefab, pos2, Quaternion.identity);

        // Εμφάνιση των κουμπιών μόλις τοποθετηθούν τα αντικείμενα
        startButton.SetActive(true);
        shootButton.SetActive(true);
        retryButton.SetActive(true);

        Debug.Log("Anchors spawned from saved positions!");
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

    // Για κάθε frame, το hosting mode ειναι ενεργό, ανιχνεύεται το tap του χρήστη, γίνεται raycast στο πλησιέστερο 
    // AR plane και δημιουργείται anchor εκεί.
    void Update()
    {
        // Έλεγχοι ότι: Βρισκόμαστε σε hosting mode, δεν αρχειοθετειται κάποιος hosting και οι anchors ειναι λιγότεροι από 2
        if (!isHosting || isWaitingForHosting || anchorCount >= 2) return;
    
        // Αποφυγή τοποθέτησης νέων anchors αν έχουν ήδη τοποθετηθεί 2
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // Raycast για ανίχνευση επιπέδου και τοποθέτηση anchor
        if (!raycastManager.Raycast(touch.position, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon)) return;

        //  Εύρεση του ARPlane που χτυπήθηκε με χρήση trackableId αν αυτό δεν είναι null
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
        // Αναμονή για το αποτέλεσμα του hosting - η εφαρμογή δεν παγώνει
        yield return promise;

        if (promise.Result.CloudAnchorState == CloudAnchorState.Success)
        {
            // αποθήκευση το ID του anchor στο Cloud
            cloudAnchorIds[anchorCount] = promise.Result.CloudAnchorId;
            anchorCount++;

            if (anchorCount < 2)
            {
                // Αν έχει τοποθετηθεί μόνο 1 anchor, ενημέρωση του χρήστη για το επόμενο anchor
                hostingStatusText.text = "Anchor 1/2  Tap για anchor 2/2";
                isWaitingForHosting = false;
            }
            else
            {
                // Αν έχουν τοποθετηθεί και τα 2 anchors, αποθήκευση των IDs για μελλοντική χρήση (resolving)
                PlayerPrefs.SetString("CloudAnchorId1", cloudAnchorIds[0]);
                PlayerPrefs.SetString("CloudAnchorId2", cloudAnchorIds[1]);
                
                PlayerPrefs.SetFloat("A1x", localAnchors[0].transform.position.rotation.x);
                PlayerPrefs.SetFloat("A1y", localAnchors[0].transform.position.rotation.y);
                PlayerPrefs.SetFloat("A1z", localAnchors[0].transform.position.rotation.z);
                PlayerPrefs.SetFloat("A2x", localAnchors[1].transform.position.rotation.x);
                PlayerPrefs.SetFloat("A2y", localAnchors[1].transform.position.rotation.y);
                PlayerPrefs.SetFloat("A2z", localAnchors[1].transform.position.rotation.z);

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
            hostingStatusText.text = "Απέτυχε! Δοκίμασε ξανά.";
            isWaitingForHosting = false;
        }
    }

    // Κλήση μόνο μετά την αρχικοποίηση των Anchors - χρήση των local anchors αντί για κλίση από το Cloud
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
        // Για περιορισμό αναμονής απάντησης από Servers
        float elapsed = 0f;

        // Αναμονή απάντησης από του Servers
        while (promise.State == PromiseState.Pending && elapsed < 60f)
        {   
            elapsed += Time.deltaTime;
            // Επιστρογή 
            yield return null;
        }

        // Αρχικοποίηση της ευρεσης των anchors με Νone
        CloudAnchorState state = CloudAnchorState.None;
        // Αν δεν βρέθηκαν anchors στα 60" τότε δεν εμφανίζει τίποτα
        try { state = promise.Result.CloudAnchorState; }
        catch (System.Exception e) { Debug.LogError("Exception: " + e.GetType().Name + " - " + e.Message); }

        // Αν βρέθηκαν anchors γίνεται αρχικοποίηση τους - Εμφανίζονται αρκιβώς εκεί που τοποθετήθηκαν κατά το hosting
        // Η πληροφορία ανακτάται από τους servers της Google και όχι από την αποθηκευμένη πληροφορία της συσκευής
        if (state == CloudAnchorState.Success)
        {
            GameObject prefab = index == 1 ? anchor1Prefab : anchor2Prefab;
            Instantiate(prefab, promise.Result.Anchor.transform.position,
                                promise.Result.Anchor.transform.rotation);
        }
    }
}
