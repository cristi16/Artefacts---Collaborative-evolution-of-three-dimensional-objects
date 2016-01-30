using UnityEngine;
using System.IO;
using System.Collections;

public class EasyDownloaderExample : MonoBehaviour {

	public string fileUrlForDownload = "http://images.persianblog.ir/208608_v9dolQPf.jpg";
	public string folderUrlForUpload = "My Uploads";
	public string fileUrlForUpload = "C:/555.jpg";
	public string phpUrlForUpload = "http://www.myweb.com/upload_file.php";
	public bool fromDisk;


	public UploadAndDownload downloader;
	
	void OnEnable()
	{
		if(downloader == null)
			Debug.LogWarning("Downloader not initialised!");
		else
		{
			UploadAndDownload.onDownloadComplete += OnDownloadComplete;
			UploadAndDownload.onUploadComplete += OnUploadComplete;
			UploadAndDownload.onError += OnError;
			UploadAndDownload.onProgress += OnProgress;
			UploadAndDownload.onUploadProgress += OnUploadProgress;
			UploadAndDownload.onSaved += OnSavedToDisk;
		}
	}
	
	void OnDisable()
	{
		UploadAndDownload.onDownloadComplete -= OnDownloadComplete;
		UploadAndDownload.onUploadComplete -= OnUploadComplete;
		UploadAndDownload.onError -= OnError;
		UploadAndDownload.onProgress -= OnProgress;
		UploadAndDownload.onUploadProgress -= OnUploadProgress;
		UploadAndDownload.onSaved -= OnSavedToDisk;
	}


	// Use this for initialization
	void Start () {
		downloader.downloadDirectory = Application.persistentDataPath;
		//Debug.Log (downloader.downloadDirectory);


	}

	void Update()
	{

	}


	// Update is called once per frame
	 void OnGUI()
	
	 {
	     fileUrlForDownload = GUILayout.TextField(fileUrlForDownload);
	     fromDisk = GUILayout.Toggle(fromDisk, "From Disk");
	     if (GUILayout.Button("Download"))
	     {
	         downloader.Download(fileUrlForDownload, true, fromDisk);
	     }
	     GUILayout.Label("\n======================================\n");
	     fileUrlForUpload = GUILayout.TextField(fileUrlForUpload);
	     if (GUILayout.Button("Upload File"))
	     {
	         UploadFile(fileUrlForUpload);
	     }
	 }


	/** Note that before using upload function your upload_file.php should be placed on your server **/
	// call this function when you want to upload your file in the form of byte array
	void UploadFile(byte[] bytes, string fileName)
	{
		if(!downloader.IsDownloading && ! downloader.IsUploading)
			downloader.UploadBytes(phpUrlForUpload, folderUrlForUpload, fileName,bytes);
	}
	// call this function when you want to upload file from disk to web server
	void UploadFile(string fileUrl)
	{
		if(!downloader.IsDownloading && ! downloader.IsUploading)
			downloader.UploadFile(phpUrlForUpload, folderUrlForUpload, fileUrl);
	}
	
	void OnError(string error)
	{
		Debug.Log("Error in downloading, Error : "+error);
	}
	
	void OnProgress(float progress)
	{
		Debug.Log("Downloading : "+progress*100+"%");
	}
	
	void OnUploadProgress(float progress)
	{
		Debug.Log("Uploading...  Please wait...");
	}
	
	void OnDownloadComplete(UploadAndDownload.Data data)
	{
		Debug.Log("Download Competed, Donwloaded bytes : "+data.bytes.Length);
		
	}
	
	void OnUploadComplete(string msg)
	{
		Debug.Log("Upload Competed : "+msg);
	}
	
	void OnSavedToDisk(string path)
	{
		Debug.Log("Data saved at path : "+path);
	}
}
