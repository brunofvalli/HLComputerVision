using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class VisionManager : MonoBehaviour
{
    #region public classes
    [System.Serializable]
    public class TagData
    {
        public string name;
        public float confidence;
    }

    [System.Serializable]
    public class AnalysedObject
    {
        public TagData[] tags;
        public string requestId;
        public object metadata;
    }
    #endregion

    #region static public field
    public static VisionManager instance;
    #endregion

    #region private fields
    // you must insert your service key here!    
    private string authorizationKey = "d7a9531eeb624802bafae9e9f4fcd1bd";
    private const string ocpApimSubscriptionKeyHeader = "df6ed739-23f7-4c0f-8020-ec0f101d9445";
    private string visionAnalysisEndpoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags";   // This is where you need to update your endpoint, if you set your location to something other than west-us.
    #endregion

    #region internal fields
    internal byte[] imageBytes;

    internal string imagePath;
    #endregion

    #region events
    private void Awake()
    {
        ConsoleOut.SendText("VisionManager Awake");
        // allows this instance to behave like a singleton
        instance = this;
    }
    #endregion

    #region private methods
    /// <summary>
    /// Returns the contents of the specified file as a byte array.
    /// </summary>
    private static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }
    #endregion

    #region public methods
    /// <summary>
    /// Call the Computer Vision Service to submit the image.
    /// </summary>
    public IEnumerator AnalyseLastImageCaptured()
    {
        ConsoleOut.SendText("AnalyseLastImageCaptured");
        WWWForm webForm = new WWWForm();

        ConsoleOut.SendText("Going to conect to : " + visionAnalysisEndpoint);
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(visionAnalysisEndpoint, webForm))
        {
            ConsoleOut.SendText("Connected to : " + visionAnalysisEndpoint);
            // gets a byte array out of the saved image
            imageBytes = GetImageAsByteArray(imagePath);
            unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
            unityWebRequest.SetRequestHeader(ocpApimSubscriptionKeyHeader, authorizationKey);

            // the download handler will help receiving the analysis from Azure
            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();

            // the upload handler will help uploading the byte array with the request
            unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
            unityWebRequest.uploadHandler.contentType = "application/octet-stream";

            yield return unityWebRequest.SendWebRequest();

            long responseCode = unityWebRequest.responseCode;

            try
            {
                string jsonResponse = null;
                jsonResponse = unityWebRequest.downloadHandler.text;

                // The response will be in Json format
                // therefore it needs to be deserialized into the classes AnalysedObject and TagData
                AnalysedObject analysedObject = new AnalysedObject();
                analysedObject = JsonUtility.FromJson<AnalysedObject>(jsonResponse);

                if (analysedObject.tags == null)
                {
                    ConsoleOut.SendText("analysedObject.tagData is null");
                    Debug.Log("analysedObject.tagData is null");
                }
                else
                {
                    ConsoleOut.SendText("analysedObject.tagData is not null getting data.");
                    Dictionary<string, float> tagsDictionary = new Dictionary<string, float>();

                    foreach (TagData td in analysedObject.tags)
                    {
                        TagData tag = td as TagData;
                        tagsDictionary.Add(tag.name, tag.confidence);
                    }

                    ResultsLabel.instance.SetTagsToLastLabel(tagsDictionary);
                }
            }
            catch (Exception exception)
            {
                Debug.Log("Json exception.Message: " + exception.Message);
            }

            yield return null;
        }
    }
    #endregion


}