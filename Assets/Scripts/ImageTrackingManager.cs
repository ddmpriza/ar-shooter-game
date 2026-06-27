using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

public class ImageTrackingManager : MonoBehaviour
{
    // Compoment του AR Foundation που ανιχνευει εικόνες μέσω της κάμερας
    private ARTrackedImageManager trackedImageManager;

    [Header("Front Camera")]                                // Οργάνωση του Inspector με [Header]
    // Panel για εμφάνιση της μπροστινής κάμερας
    [SerializeField] private GameObject frontCameraPanel;
    // RawImage για εμφάνιση της μπροστινής κάμερας - RawImage για χρήση texture (WebCamTexture)
    [SerializeField] private RawImage frontCameraDisplay;

    [Header("Video")]
    // Panel για εμφάνιση video
    [SerializeField] private GameObject videoPanel;
    // VideoPlayer για αναπαραγωγή video - Αρχιοθέτηση στο Start() για αποφυγή null reference error
    private UnityEngine.Video.VideoPlayer videoPlayer;

    [Header("Score")]
    // Panel για εμφάνιση score
    [SerializeField] private GameObject scorePanel;
    // Text component για εμφάνιση score
    [SerializeField] private TextMeshProUGUI scorePanelText;

    // Μέσω μεταφοράς live video από την μπροστινή κάμερα της συσκευής στο RawImage (frontCameraDisplay)
    private WebCamTexture frontCamTexture;

    void Awake()
    {
        // Εξάρτηση από το ARTrackedImageManager component που βρίσκεται στο ίδιο GameObject (XR Origin)
        // trackedImageManager: Αναφορά στο component (βιβλιοθήκη) που ανιχνεύει εικόνες μέσω της κάμερας
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void Start()
    {
        // Έλεγχος επιτυχής δημιουργιας videoPlayer
        if (videoPanel != null)
            videoPlayer = videoPanel.GetComponent<UnityEngine.Video.VideoPlayer>();
    }

    // Κλήση όταν ανιχευτεί συγκεκριμένη εικόνα μέσω της κάμερας (ARTrackedImageManager)
    // Ξεκινά με την έναρξη της σκήνης (SampleScene) και παρακολουθεί συνεχώς τις εικόνες που ανιχνεύονται
    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    // Όταν αλλάξει η σκηνή απενεργοποιείται η παρακολούθηση εικόνων
    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    // Μέθοδος που καλείται κάθε φορά με την εναλλαγή των ανιχνευμένων εικόνων (προσθήκη, ενημέρωση, αφαίρεση)
    void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Εικόνες που ανιχνεύτηκαν για πρώτη φορά (προστέθηκαν)
        foreach (var trackedImage in eventArgs.added)
        {
            HandleImage(trackedImage);
        }

        // Εικόνες που ήδη ανιχνεύονται και ενημερώνεται η θέση τους (updated)
        foreach (var trackedImage in eventArgs.updated)
        {
            HandleImage(trackedImage);
        }

        // Εικόνες που δεν ανιχνεύονται πλέον (αφαιρέθηκαν)
        foreach (var trackedImage in eventArgs.removed)
        {
            // Απόκρυψη όλων των panels όταν αφαιρεθεί η εικόνα
            HideAll();
        }
    }

    void HandleImage(ARTrackedImage trackedImage)
    {
        // Λειτουργία του ImageTrackingManager μόνο όταν το game έχει ξεκινήσει (gameStarted = true)
        if (!GameStateManager.instance.gameStarted) return;

        // Λήψη του ονόματος της εικόνας που ανιχνεύτηκε 
        // Τα ονόματα των εικόνων καθως και οι εικόνες που ανιχνεύονται βρίσκονται στο AR Reference Image Library (Assets/ARResources/ImageLibrary)
        string name = trackedImage.referenceImage.name;

        // Απόκρυψη τυχόν υπάρχων Panels πριν εμφανίσει το νέο panel που αντιστοιχεί στην ανιχνευμένη εικόνα
        HideAll();

        // Εκτέλεση διαφορετικής ενέργειας ανάλογα με το όνομα της εικόνας που ανιχνεύτηκε
        if (name == "front_camera")
        {
            frontCameraPanel.SetActive(true);
            StartFrontCamera();
        }
        else if (name == "score")
        {
            scorePanel.SetActive(true);
            scorePanelText.text = "Score: "  + ScoreManager.instance.GetScore();
        }
        else if (name == "video")
        {
            videoPanel.SetActive(true);
            videoPlayer.Stop();
            videoPlayer.Play();
        }
    }

    void StartFrontCamera()
    {
        // Για κάθε διαθέσιμη κάμερα της συσκευής
        foreach (var device in WebCamTexture.devices)
        {
            // Ελέγχουμε αν η κάμερα είναι μπροστινή (front facing)
            if (device.isFrontFacing)
            {
                // Δημιουργία νέου Texture για την μπροστινή κάμερα (WebCamTexture)
                frontCamTexture = new WebCamTexture(device.name);
                // Σύνδεση της μπροστινής κάμερας στο RawImage (frontCameraDisplay) μέσω WebCamTexture
                frontCameraDisplay.texture = frontCamTexture;
                // Εκκίνηση της αναπαραγωγής της μπροστινής κάμερας
                frontCamTexture.Play();
                // Αν ξεκινήσει η εγγραφή της μπροστινής κάμερας, σταματάει η αναζήτηση για άλλες κάμερες
                break;
            }
        }
    }

    // Απόκρυψη όλων των Panels 
    void HideAll()
    {
        frontCameraPanel.SetActive(false);
        videoPanel.SetActive(false);
        videoPlayer.Stop();
        scorePanel.SetActive(false);

        // Ακόμα και αν η μπροστινή κάμερα δεν άνοιξε σωστά αλλά εκτελείται
        if (frontCamTexture != null && frontCamTexture.isPlaying)
            frontCamTexture.Stop();
    }
}