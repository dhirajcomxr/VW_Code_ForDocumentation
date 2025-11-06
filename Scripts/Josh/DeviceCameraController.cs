using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;

public class DeviceCameraController : MonoBehaviour
{
    public RawImage image;
    public AspectRatioFitter imageFitter;

    //set it to either FRONT or BACK
    string myCamera = "BACK";

    // Device cameras
    WebCamDevice frontCameraDevice;
    WebCamDevice backCameraDevice;
    WebCamDevice activeCameraDevice;

    WebCamTexture frontCameraTexture;
    WebCamTexture backCameraTexture;
    WebCamTexture activeCameraTexture;

    // Image rotation
    Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image uvRect
    Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
    Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

    // Image Parent's scale
    Vector3 defaultScale = new Vector3(1f, 1f, 1f);
    Vector3 fixedScale = new Vector3(-1f, 1f, 1f);

    void Start()
    {
        totalCams = WebCamTexture.devices.Length;
        // Check for device cameras
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.Log("No devices cameras found");
            return;
        }

        // Get the device's cameras and create WebCamTextures with them
        frontCameraDevice = WebCamTexture.devices.Last();
        backCameraDevice = WebCamTexture.devices.First();

      //  frontCameraTexture = new WebCamTexture(frontCameraDevice.name);
        frontCameraTexture = new WebCamTexture(frontCameraDevice.name, 640, 480);
      //  backCameraTexture = new WebCamTexture(backCameraDevice.name);
        backCameraTexture = new WebCamTexture(backCameraDevice.name, 640, 480);

        // Set camera filter modes for a smoother looking image
        frontCameraTexture.filterMode = FilterMode.Trilinear;
        backCameraTexture.filterMode = FilterMode.Trilinear;

        // Set the camera to use by default
        if (myCamera.Equals("FRONT"))
            SetActiveCamera(frontCameraTexture);
        else if (myCamera.Equals("BACK"))
            SetActiveCamera(backCameraTexture);
        else // default back
            SetActiveCamera(backCameraTexture);
    }
    [SerializeField] int curCam = 0, totalCams = 0;
    public void ToggleCamera()
    {
        curCam++;
        if (curCam >= WebCamTexture.devices.Length)
            curCam = 0;
        SetCamera(curCam);
    }
    public void SetCamera(int d)
    {
        if (WebCamTexture.devices.Length < d)
        {
            Debug.Log("No devices cameras found");
            return;

        }
        WebCamDevice camDevice = WebCamTexture.devices[d];
     //   WebCamTexture camTex = new WebCamTexture(camDevice.name);
        WebCamTexture camTex = new WebCamTexture(camDevice.name,640,480);
        camTex.filterMode = FilterMode.Trilinear;
        SetActiveCamera(camTex);
    }
    // Set the device camera to use and start it
    public void SetActiveCamera(WebCamTexture cameraToUse)
    {
        if (activeCameraTexture != null)
        {
            activeCameraTexture.Stop();
        }

        activeCameraTexture = cameraToUse;
        activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device =>
            device.name == cameraToUse.deviceName);

        image.texture = activeCameraTexture;
        image.material.mainTexture = activeCameraTexture;

        activeCameraTexture.Play();
    }

    // Make adjustments to image every frame to be safe, since Unity isn't 
    // guaranteed to report correct data as soon as device camera is started
    void Update()
    {
        // Skip making adjustment for incorrect camera data
        if (activeCameraTexture.width < 100)
        {
            Debug.Log("Still waiting another frame for correct info...");
            return;
        }

        // Rotate image to show correct orientation 
        rotationVector.z = -activeCameraTexture.videoRotationAngle;
        image.rectTransform.localEulerAngles = rotationVector;

        // Set AspectRatioFitter's ratio
        float videoRatio =
            (float)activeCameraTexture.width / (float)activeCameraTexture.height;
        imageFitter.aspectRatio = videoRatio;

        // Unflip if vertically flipped
        image.uvRect =
            activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

    }
}