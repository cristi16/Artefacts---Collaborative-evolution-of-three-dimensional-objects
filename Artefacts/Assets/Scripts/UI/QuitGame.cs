using UnityEngine;
using System.Collections;

public class QuitGame : MonoBehaviour {

    public void Quit()
    {
        if (ArtefactEvolver.Instance != null)
            ArtefactEvolver.Instance.SaveStatistics();
        StartCoroutine(StartQuit());
    }

    IEnumerator StartQuit()
    {
        while (FileUploader.isUploading)
        {
            yield return null;
        }
        Application.Quit();
    }
}
