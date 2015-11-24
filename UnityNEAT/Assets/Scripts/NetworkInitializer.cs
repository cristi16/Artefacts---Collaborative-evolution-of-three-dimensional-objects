using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class NetworkInitializer : MonoBehaviour
{
    public Text welcomeBackText;
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
        // This allows us to send as much data as possible. However this means that connection to server will be slow if the servers keeps track of a lot of objects
        networkManager.connectionConfig.MaxSentMessageQueueSize = UInt16.MaxValue;

        // Set name field if name has been previously saved
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            nameField.gameObject.SetActive(false);
            welcomeBackText.gameObject.SetActive(true);
            welcomeBackText.text += PlayerPrefs.GetString("PlayerName");
        }

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
        connectButton.interactable = nameField.text != string.Empty;
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
        PlayerPrefs.SetString("PlayerName" , nameField.text);
        networkManager.PlayerName = nameField.text;
        networkManager.StartClient();
    }
}
