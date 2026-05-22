using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

public class ImageTrackingManager : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    [Header("Among Us - Front Camera")]
    [SerializeField] private GameObject frontCameraPanel;
    [SerializeField] private RawImage frontCameraDisplay;

    [Header("YouTube - Video")]
    [SerializeField] private GameObject videoPanel;

    [Header("Olympics - Score")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private TextMeshProUGUI scorePanelText;

    private WebCamTexture frontCamTexture;

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
        if (!GameStateManager.instance.gameStarted) return;
        if (trackedImage.trackingState != TrackingState.Tracking) return;

        string name = trackedImage.referenceImage.name;

        HideAll();

        if (name == "AmongUs")
        {
            frontCameraPanel.SetActive(true);
            StartFrontCamera();
        }
        else if (name == "YouTube")
        {
            videoPanel.SetActive(true);
        }
        else if (name == "Olympics")
        {
            scorePanel.SetActive(true);
            scorePanelText.text = "Score: " + ScoreManager.instance.GetScore();
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
        scorePanel.SetActive(false);

        if (frontCamTexture != null && frontCamTexture.isPlaying)
            frontCamTexture.Stop();
    }
}