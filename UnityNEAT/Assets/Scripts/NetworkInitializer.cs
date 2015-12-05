using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;
using UnityEngine.UI;

public class NetworkInitializer : MonoBehaviour
{
    public InputField ipField;
    public Text welcomeBackText;
    public InputField nameField;
    public Button connectButton;
    [SerializeField] private bool autoConnect = true;
    [SerializeField] private bool serverInEditor = true;

    public bool useMatchmaking = false;
    public bool hostMatchmakingGame = false;

    private CustomNetworkManager networkManager;

    private enum ConnectionType { Client, Host, Server}

    void Start ()
	{
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        networkManager = FindObjectOfType<CustomNetworkManager>();
        // This allows us to send as much data as possible. However this means that connection to server will be slow if the servers keeps track of a lot of objects
        networkManager.connectionConfig.MaxSentMessageQueueSize = UInt16.MaxValue;

        ActivateInputField();

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
	                ConnectAs(ConnectionType.Host);
	            else
	                ConnectAs(ConnectionType.Client);
	        }
	        else
	        {
	            if (Application.isEditor)
	                ConnectAs(ConnectionType.Client);
	            else
	                ConnectAs(ConnectionType.Host);
	        }
	    }
	}

    public void ActivateInputField()
    {
        // Set name field if name has been previously saved
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            nameField.gameObject.SetActive(false);
            welcomeBackText.gameObject.SetActive(true);
            welcomeBackText.text += PlayerPrefs.GetString("PlayerName");
        }

        nameField.Select();
        nameField.ActivateInputField();
    }

    public void ShowInputField()
    {
        nameField.text = "";
        nameField.gameObject.SetActive(true);
        welcomeBackText.gameObject.SetActive(false);

        nameField.Select();
        nameField.ActivateInputField();
    }

    void Update()
    {
        if(nameField.gameObject.activeSelf)
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
        ConnectAs(ConnectionType.Client);   
    }

    public void ConnectAsHost()
    {
        ConnectAs(ConnectionType.Host);
    }

    private void ConnectAs(ConnectionType connectionType, MatchInfo matchInfo = null)
    {
        networkManager.networkAddress = ipField.text;

        switch (connectionType)
        {
                case ConnectionType.Client:
                    
                    SavePlayerName();
                    if (matchInfo != null)
                        networkManager.StartClient(matchInfo);
                    else
                        networkManager.StartClient();
                    break;

                case ConnectionType.Host:
                    
                    SavePlayerName();
                    if (matchInfo != null)
                        networkManager.StartHost(matchInfo);
                    else
                        networkManager.StartHost();
                    break;

                case ConnectionType.Server:

                    networkManager.StartServer();

                break;
        }
    }

    private void SavePlayerName()
    {
        if (PlayerPrefs.HasKey("PlayerName") == false)
        {
            PlayerPrefs.SetString("PlayerName", nameField.text);
        }
    }
}
