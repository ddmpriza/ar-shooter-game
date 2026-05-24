using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;

// Διαχειρστής για hosting και resolving cloud anchors
// Υλοποίσηση δύο λειτουργιών: 
// Hsotinf: Ο χρήστης τοποθετεί 2 anchors και τα ανεβάζει στο cloud
// Resolving: Οι χρήστες προσπαθούν να βρουν τα anchors που έχουν ανεβεί
public class CloudAnchorManager : MonoBehaviour
{
    public static CloudAnchorManager instance;

    [Header("AR Components")]
    // Αναφορά στον ARAnchorManager για hosting/resolving
    [SerializeField] private ARAnchorManager anchorManager;
    // Αναφορά στον ARRaycastManager για ανίχνευση planes και τοποθέτηση anchors
    [SerializeField] private ARRaycastManager raycastManager;

    [Header("Objects to spawn")]
    // 3D μοντέλο που εμφανίζεται στο πρώτο anchor
    [SerializeField] private GameObject anchor1Prefab;
    // Glossy object που εμφανίζεται στο δεύτερο anchor
    [SerializeField] private GameObject anchor2Prefab;

    // Λίστα αποθήκευσης αποτελεσμάτων raycast
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    
    // IDs των cloud anchors που θα αποθηκευτούν μετά το hosting
    private string cloudAnchorId1;
    private string cloudAnchorId2;

    // Μετρητής τοποθέτησης anchors 
    private int anchorCount = 0;
    // Flag σηματοδότησης hosting λειτουργίας
    // Αυτό θα χρησιμοποιηθεί από το PlaceObject για να απενεργοποιήσει την τοποθέτηση νέων objects κατά τη διάρκεια του hosting
    public bool isHosting = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        Debug.Log("StartHosting flag: " + PlayerPrefs.GetInt("StartHosting", 0));
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
            ResolveAnchors();
        }
    }

    // Ενεργοποίηση hosting λειτουργίας - καλείται από το MenuManager όταν ο χρήστης επιλέγει να ξεκινήσει hosting
    public void StartHosting()
    {
        isHosting = true;
        anchorCount = 0;
        Debug.Log("Hosting mode started - tap to place 2 anchors");
    }

    void Update()
    {
        // Έλεγχος Hosting mode
        if (!isHosting) return;

        // Αποφυγή τοποθέτησης νέων anchors αν έχουν ήδη τοποθετηθεί 2
        if (Input.touchCount == 0) return;

        // Περιορισμός σε 2 anchors για hosting
        if (anchorCount >= 2) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // Raycast για ανίχνευση επιπέδου και τοποθέτηση anchor
        if (raycastManager.Raycast(touch.position, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
        {
            Debug.Log("Raycast hit plane! Hosting anchor...");
            HostAnchor(hits[0].pose);
        }
        else
        {
            Debug.Log("Raycast missed - no plane detected");
        }
    }

    // Δημιουργία anchor στο σημείο που χτύπησε ο χρήστης και έναρξη της διαδικασίας hosting
    private void HostAnchor(Pose pose)
    {
        // Δημιουργία κενού GameObject στην θέση του tap
        GameObject anchorGO = new GameObject("CloudAnchor");
        anchorGO.transform.position = pose.position;
        anchorGO.transform.rotation = pose.rotation;

        // Προσθήκη ARAnchor component για να γίνει anchor
        ARAnchor anchor = anchorGO.AddComponent<ARAnchor>();

        // Upload του anchor στο cloud
        HostCloudAnchorPromise promise = anchorManager.HostCloudAnchorAsync(anchor, 1);
        StartCoroutine(WaitForHosting(promise));
    }

    // Aναμονή για το αποτέλεσμα του hosting και αποθήκευση των IDs των anchors
    private IEnumerator WaitForHosting(HostCloudAnchorPromise promise)
    {
        // Αναμονή για το αποτέλεσμα του hosting
        yield return promise;

        if (promise.Result.CloudAnchorState == CloudAnchorState.Success)
        {
            anchorCount++;
            string id = promise.Result.CloudAnchorId;
            Debug.Log("Cloud Anchor hosted! ID: " + id);

            // Αποθήκευση του ID του cloud anchor αν το hosting ήταν επιτυχές ανάλογα με ποίο anchor είναι (1 ή 2)
            if (anchorCount == 1)
                cloudAnchorId1 = id;
            else if (anchorCount == 2)
            {
                cloudAnchorId2 = id;
                isHosting = false;
                Debug.Log("Both anchors hosted!");
            }

            // Αν και τα δύο anchors έχουν hosted, αποθήκευση των IDs στα PlayerPrefs για να είναι διαθέσιμα κατά το resolving
            PlayerPrefs.SetString("CloudAnchorId1", cloudAnchorId1);
            PlayerPrefs.SetString("CloudAnchorId2", cloudAnchorId2);
            PlayerPrefs.Save();
            Debug.Log("Anchor " + anchorCount + " saved!");
        }
        else
        {
            Debug.LogError("Hosting failed: " + promise.Result.CloudAnchorState);
            isHosting = false;
        }
    }

    // Εύρεση και εμφάνιση των anchors που έχουν αποθηκευτεί στο cloud anchors
    // Καλείται κατα το Game Mode 
    public void ResolveAnchors()
    {
        // Ανάκτηση των αποθηκευμένων IDs των cloud anchors από τα PlayerPrefs
        cloudAnchorId1 = PlayerPrefs.GetString("CloudAnchorId1", "");
        cloudAnchorId2 = PlayerPrefs.GetString("CloudAnchorId2", "");

        // Αν δεν υπάρχουν αποθηκευμένα IDs, εμφάνιση μηνύματος και έξοδος
        if (cloudAnchorId1 != "")
            StartCoroutine(WaitForResolving( anchorManager.ResolveCloudAnchorAsync(cloudAnchorId1), 1));
        
        if (cloudAnchorId2 != "")
            StartCoroutine(WaitForResolving(anchorManager.ResolveCloudAnchorAsync(cloudAnchorId2), 2));
    }

    // Αναμονή αποτελέσματος του resolving και εμφάνιση των αντικειμένων στα σημεία των anchors αν το resolving ήταν επιτυχές
    private IEnumerator WaitForResolving(ResolveCloudAnchorPromise promise, int anchorIndex)
    {
        // Αναμονή για την εύρεση anchor στον χώρο
        yield return promise;

        // Αν βρεθεί
        if (promise.Result.CloudAnchorState == CloudAnchorState.Success)
        {
            Debug.Log("Anchor " + anchorIndex + " resolved!");
            // Εμφάνιση του αντικειμένου στο σημείο του anchor ανάλογα με το ποιο anchor είναι (1 ή 2)
            if (anchorIndex == 1)
                Instantiate(anchor1Prefab, promise.Result.Anchor.transform);
            else
                Instantiate(anchor2Prefab, promise.Result.Anchor.transform);
        }
        else
            Debug.LogError("Resolving failed: " + promise.Result.CloudAnchorState);
    }
}
