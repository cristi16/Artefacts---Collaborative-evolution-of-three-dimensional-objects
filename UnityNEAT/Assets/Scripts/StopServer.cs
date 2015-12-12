using UnityEngine;
using System.Collections;
using UnityStandardAssets.Network;

public class StopServer : MonoBehaviour
{

    public void Stop()
    {
        LobbyManager.s_Singleton.GoBackButton();

    }
}
