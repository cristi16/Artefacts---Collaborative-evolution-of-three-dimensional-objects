using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetworkInitializer : MonoBehaviour
{
    [SerializeField] private bool serverInEditor = true;
	
	void Start ()
	{
	    var networkManager = FindObjectOfType<NetworkManager>();

        if (serverInEditor)
        {
            if (Application.isEditor)
                networkManager.StartHost();
            else
                networkManager.StartClient();
        }
	    else
	    {
            if (Application.isEditor)
                networkManager.StartClient();
            else
                networkManager.StartHost();
        }
	}	
}
