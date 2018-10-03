using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.WSA.Input;
using UnityEngine.XR.WSA.WebCam;

public class ImageCapture : MonoBehaviour {

    public static ImageCapture instance;
    public int tapsCount;
    private PhotoCapture photoCaptureObject = null;
    private GestureRecognizer recognizer;
    private bool currentlyCapturing = false;

    private void Awake()
    {
        // Allows this instance to behave like a singleton
        instance = this;

        ConsoleOut.SendText("Awake Image Capture");
    }

    void Start()
    {
        ConsoleOut.SendText("Star ImageCapture");

        // subscribing to the Hololens API gesture recognizer to track user gestures
        recognizer = new GestureRecognizer();
        recognizer.SetRecognizableGestures(GestureSettings.Tap);
        recognizer.Tapped += TapHandler;
        recognizer.StartCapturingGestures();
    }

    /// <summary>
    /// Respond to Tap Input.
    /// </summary>
    private void TapHandler(TappedEventArgs obj)
    {
        ConsoleOut.SendText("TapHandler");
        // Only allow capturing, if not currently processing a request.
        if (currentlyCapturing == false)
        {
            currentlyCapturing = true;

            // increment taps count, used to name images when saving
            tapsCount++;

            // Create a label in world space using the ResultsLabel class
            ResultsLabel.instance.CreateLabel();

            // Begins the image capture and analysis procedure
            ExecuteImageCaptureAndAnalysis();
        }
    }

    /// <summary>
    /// Register the full execution of the Photo Capture. If successful, it will begin 
    /// the Image Analysis process.
    /// </summary>
    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        ConsoleOut.SendText("OnCapturedPhotoToDisk");
        // Call StopPhotoMode once the image has successfully captured
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        ConsoleOut.SendText("OnStoppedPhoteMode");
        // Dispose from the object in memory and request the image analysis 
        // to the VisionManager class
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
        StartCoroutine(VisionManager.instance.AnalyseLastImageCaptured());
    }

    /// <summary>    
    /// Begin process of Image Capturing and send To Azure     
    /// Computer Vision service.   
    /// </summary>    
    private void ExecuteImageCaptureAndAnalysis()
    {
        ConsoleOut.SendText("ExecuteImageCaptureAndAnalysis");

        ConsoleOut.SendText("Resolutions " + PhotoCapture.SupportedResolutions.Count());

        // Set the camera resolution to be the highest possible    
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        ConsoleOut.SendText("before CreateAsync");
        // Begin capture process, set the image format    
        PhotoCapture.CreateAsync(false, delegate (PhotoCapture captureObject)
        {
            ConsoleOut.SendText("after CreateAsync");
            photoCaptureObject = captureObject;
            CameraParameters camParameters = new CameraParameters();
            camParameters.hologramOpacity = 0.0f;
            camParameters.cameraResolutionWidth = targetTexture.width;
            camParameters.cameraResolutionHeight = targetTexture.height;
            camParameters.pixelFormat = CapturePixelFormat.BGRA32;

            // Capture the image from the camera and save it in the App internal folder    
            captureObject.StartPhotoModeAsync(camParameters, delegate (PhotoCapture.PhotoCaptureResult result)
            {
                ConsoleOut.SendText("end StartPhotoModeAsync");
                string filename = string.Format(@"CapturedImage{0}.jpg", tapsCount);

                string filePath = Path.Combine(Application.persistentDataPath, filename);

                VisionManager.instance.imagePath = filePath;

                photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.JPG, OnCapturedPhotoToDisk);

                currentlyCapturing = false;
            });
        });
    }
}
