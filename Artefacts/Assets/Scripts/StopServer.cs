using UnityEngine;
using System.Collections;
using UnityStandardAssets.Network;

public class StopServer : MonoBehaviour
{
    public GameObject panel;

    public void Stop()
    {
        if(ArtefactEvolver.Instance != null)
            ArtefactEvolver.Instance.SaveStatistics();
        StartCoroutine(StartStopping());
    }

    IEnumerator StartStopping()
    {
        while (FileUploader.isUploading)
        {
            yield return null;
        }
        LobbyManager.s_Singleton.GoBackButton();
        panel.SetActive(false);
    }
}
