using UnityEngine;
using System.IO;
using System.Collections;

[RequireComponent(typeof(UploadAndDownload))]
public class FileUploader : MonoBehaviour
{
    public string folderUrlForUpload = "screenshots";
    private string phpUrlForUpload = "http://evolve3d.mygamesonline.org/upload_file.php";

    private UploadAndDownload uploader;

    public static bool isUploading = false;

    void OnEnable()
    {
        uploader = GetComponent<UploadAndDownload>();

        if (uploader == null)
            Debug.LogWarning("Downloader not initialised!");
        else
        {
            UploadAndDownload.onUploadComplete += OnUploadComplete;
            UploadAndDownload.onError += OnError;
            UploadAndDownload.onUploadProgress += OnUploadProgress;
        }
    }

    void OnDisable()
    {
        UploadAndDownload.onUploadComplete -= OnUploadComplete;
        UploadAndDownload.onError -= OnError;
        UploadAndDownload.onUploadProgress -= OnUploadProgress;
    }

    /** Note that before using upload function your upload_file.php should be placed on your server **/
    // call this function when you want to upload your file in the form of byte array
    public void UploadFile(byte[] bytes, string fileName)
    {
        if (!uploader.IsDownloading && !uploader.IsUploading)
        {
            uploader.UploadBytes(phpUrlForUpload, folderUrlForUpload, fileName, bytes);
            isUploading = true;
        }
    }

    // call this function when you want to upload file from disk to web server
    public void UploadFile(string fileUrl)
    {
        if (!uploader.IsDownloading && !uploader.IsUploading)
        {
            uploader.UploadFile(phpUrlForUpload, folderUrlForUpload, fileUrl);
            isUploading = true;
        }
    }

    void OnError(string error)
    {
        isUploading = false;
        Debug.Log("Error : " + error);
    }

    void OnUploadComplete(string msg)
    {
        Debug.Log("Upload Completed : " + msg);
        isUploading = false;
    }

    void OnUploadProgress(float progress)
    {
        
    }
}
