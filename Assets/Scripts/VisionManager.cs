using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


public class VisionManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

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
public static VisionManager instance;

// you must insert your service key here!
private string authorizationKey = "083a45e30e654c33b6796066e2dd09ee";

private const string ocpApimSubscriptionKeyHeader = "Ocp-Apim-Subscription-Key";

private string visionAnalysisEndpoint =
"https://westus.api.cognitive.microsoft.com/vision/v1.0";

[HideInInspector] public byte[] imageBytes;
[HideInInspector] public string imagePath;

private void Awake()
{
    // allows this instance to behave like a singleton
    instance = this;
}

/// <summary>
/// Call the Computer Vision Service to submit the image.
/// </summary>
public IEnumerator AnalyseLastImageCaptured()
{
    WWWForm webForm = new WWWForm();

    using (UnityWebRequest unityWebRequest =
    UnityWebRequest.Post(visionAnalysisEndpoint, webForm))
    {
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
            // therefore it needs to be deserialized into the classes AnalysedObject and Tag-Data
            AnalysedObject analysedObject = new AnalysedObject();
            analysedObject = JsonUtility.FromJson<AnalysedObject>(jsonResponse);
            if (analysedObject.tags == null)
            {
                Debug.Log("analysedObject.tagData is null");
            }
            else
            {
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

    private byte[] GetImageAsByteArray(string imagePath)
    {
        throw new NotImplementedException();
    }
}
