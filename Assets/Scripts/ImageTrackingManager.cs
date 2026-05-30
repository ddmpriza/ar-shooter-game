using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

public class ImageTrackingManager : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    [Header("Front Camera")]
    [SerializeField] private GameObject frontCameraPanel;
    [SerializeField] private RawImage frontCameraDisplay;

    [Header("Video")]
    [SerializeField] private GameObject videoPanel;
    private UnityEngine.Video.VideoPlayer videoPlayer;

    [Header("Score")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private TextMeshProUGUI scorePanelText;

    private WebCamTexture frontCamTexture;

    void Start()
    {
        if (videoPanel != null)
            videoPlayer = videoPanel.GetComponent<UnityEngine.Video.VideoPlayer>();
    }

    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            HandleImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            HandleImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.removed)
        {
            HideAll();
        }
    }

    void HandleImage(ARTrackedImage trackedImage)
    {
        Debug.Log("Tracked image: " + trackedImage.referenceImage.name);
        Debug.Log("gameStarted: " + GameStateManager.instance.gameStarted);
        Debug.Log("trackingState: " + trackedImage.trackingState);

        if (!GameStateManager.instance.gameStarted) return;
        //if (trackedImage.trackingState != TrackingState.Tracking) return;

        string name = trackedImage.referenceImage.name;

        HideAll();

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
            Debug.Log("Video case triggered! videoPanel: " + (videoPanel != null) + 
              " videoPlayer: " + (videoPlayer != null));
            videoPanel.SetActive(true);
            videoPlayer.Stop();
            videoPlayer.Play();
        }
    }

    void StartFrontCamera()
    {
        foreach (var device in WebCamTexture.devices)
        {
            if (device.isFrontFacing)
            {
                frontCamTexture = new WebCamTexture(device.name);
                frontCameraDisplay.texture = frontCamTexture;
                frontCamTexture.Play();
                break;
            }
        }
    }

    void HideAll()
    {
        frontCameraPanel.SetActive(false);
        videoPanel.SetActive(false);
        videoPlayer.Stop();
        scorePanel.SetActive(false);

        if (frontCamTexture != null && frontCamTexture.isPlaying)
            frontCamTexture.Stop();
    }
}