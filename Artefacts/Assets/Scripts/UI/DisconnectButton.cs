using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DisconnectButton : MonoBehaviour {

    public void Disconnect()
    {
        NetworkManager.singleton.StopClient();
        transform.parent.gameObject.SetActive(false);
    }

    public void ReturnBack()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
