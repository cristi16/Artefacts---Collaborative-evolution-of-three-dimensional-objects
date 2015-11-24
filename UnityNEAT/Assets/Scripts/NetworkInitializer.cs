using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class NetworkInitializer : MonoBehaviour
{
    public InputField nameField;
    public Button connectButton;
    [SerializeField] private bool autoConnect = true;
    [SerializeField] private bool serverInEditor = true;

    public bool useMatchmaking = false;
    public bool hostMatchmakingGame = false;

    private CustomNetworkManager networkManager;

    void Start ()
	{
	    networkManager = FindObjectOfType<CustomNetworkManager>();
        networkManager.connectionConfig.MaxSentMessageQueueSize = UInt16.MaxValue;

        // Use the matchmaker
        if (useMatchmaking)
        {
            networkManager.StartMatchMaker();
            networkManager.matchMaker.SetProgramAppID((AppID)359252);
            if (hostMatchmakingGame)
                networkManager.matchMaker.CreateMatch("Default", 100, false, "", networkManager.OnMatchCreate);
            else
                networkManager.matchMaker.ListMatches(0, 6, "", OnMatchList);
        }


        if (autoConnect)
	    {
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

    void Update()
    {
        //connectButton.isActiveAndEnabled = false;
    }

    void OnMatchList(ListMatchResponse matchList)
    {
        if (matchList.success)
        {
            networkManager.matchMaker.JoinMatch(matchList.matches[0].networkId, "", networkManager.OnMatchJoined);
        }
        else
        {
            Debug.LogError("Problem finding a match to join!");
        }
    }

    public void ConnectToServer()
    {
        PlayerPrefs.SetString("PlayerName" ,nameField.text);
        networkManager.StartClient();
    }
}
