using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace UnityStandardAssets.Network
{
    public class LobbyManager : NetworkLobbyManager 
    {
        static public LobbyManager s_Singleton;

        public LobbyTopPanel topPanel;

        public RectTransform mainMenuPanel;
        public RectTransform lobbyPanel;

        public LobbyInfoPanel infoPanel;

        protected RectTransform currentPanel;

        public Button backButton;

        public Text statusInfo;
        public Text hostInfo;

        //used to disconnect a client properly when exiting the matchmaker
        public bool isMatchmaking = false;
        protected bool _disconnectServer = false;
        
        protected ulong _currentMatchID;

        protected LobbyHook _lobbyHooks;


        void Start()
        {
            s_Singleton = this;
            connectionConfig.IsAcksLong = true;
            connectionConfig.MaxSentMessageQueueSize = 512;

            _lobbyHooks = GetComponent<UnityStandardAssets.Network.LobbyHook>();
            currentPanel = mainMenuPanel;

            backButton.gameObject.SetActive(false);
            GetComponent<Canvas>().enabled = true;

            DontDestroyOnLoad(gameObject);

            SetServerInfo("Offline", "None");
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);

            Type serverType = typeof(NetworkServer);
            FieldInfo info = serverType.GetField("maxPacketSize",
                BindingFlags.NonPublic | BindingFlags.Static);
            ushort maxPackets = 1500;
            info.SetValue(null, maxPackets);
        }

        public override void OnLobbyClientConnect(NetworkConnection conn)
        {
            base.OnLobbyClientConnect(conn);
            Type serverType = typeof(NetworkServer);
            FieldInfo info = serverType.GetField("maxPacketSize",
                BindingFlags.NonPublic | BindingFlags.Static);
            ushort maxPackets = 1500;
            info.SetValue(null, maxPackets);
        }

        public override void OnLobbyClientSceneChanged(NetworkConnection conn)
        {
            if (!conn.playerControllers[0].unetView.isLocalPlayer)
                return;

            if (Application.loadedLevelName == lobbyScene)
            {
                if (topPanel.isInGame)
                {
                    ChangeTo(lobbyPanel);
                    if (isMatchmaking)
                    {
                        if (conn.playerControllers[0].unetView.isServer)
                        {
                            backDelegate = StopHostClbk;
                        }
                        else
                        {
                            backDelegate = StopClientClbk;
                        }
                    }
                    else
                    {
                        if (conn.playerControllers[0].unetView.isClient)
                        {
                            backDelegate = StopHostClbk;
                        }
                        else
                        {
                            backDelegate = StopClientClbk;
                        }
                    }
                }
                else
                {
                    ChangeTo(mainMenuPanel);
                }

                topPanel.isInGame = false;
                topPanel.ToggleVisibility(true);
            }
            else
            {
                ChangeTo(null);

                Destroy(GameObject.Find("MainMenuUI(Clone)"));

                backDelegate = StopGameClbk;
                topPanel.isInGame = true;
                topPanel.ToggleVisibility(false);
            }
        }

        public void ChangeTo(RectTransform newPanel)
        {
            if (currentPanel != null)
            {
                currentPanel.gameObject.SetActive(false);
            }

            if (newPanel != null)
            {
                newPanel.gameObject.SetActive(true);
            }

            currentPanel = newPanel;

            if (currentPanel != mainMenuPanel)
            {
                backButton.gameObject.SetActive(true);
            }
            else
            {
                backButton.gameObject.SetActive(false);
                SetServerInfo("Offline", "None");
                isMatchmaking = false;
            }
        }

        public void DisplayIsConnecting()
        {
            var _this = this;
            infoPanel.Display("Connecting...", "Cancel", () => { _this.backDelegate(); });
        }

        public void SetServerInfo(string status, string host)
        {
            statusInfo.text = status;
            hostInfo.text = host;
        }


        public delegate void BackButtonDelegate();
        public BackButtonDelegate backDelegate;
        public void GoBackButton()
        {
            backDelegate();
        }

        // ----------------- Server management

        public void SimpleBackClbk()
        {
            ChangeTo(mainMenuPanel);
        }

        public void StopHostClbk()
        {
            if (isMatchmaking)
            {
                this.matchMaker.DestroyMatch((NetworkID)_currentMatchID, OnMatchDestroyed);
                _disconnectServer = true;
            }
            else
            {
                StopHost();
            }

            
            ChangeTo(mainMenuPanel);
        }

        public void StopClientClbk()
        {
            StopClient();

            if (isMatchmaking)
            {
                StopMatchMaker();
            }

            ChangeTo(mainMenuPanel);
        }

        public void StopServerClbk()
        {
            StopServer();
            ChangeTo(mainMenuPanel);
        }

        public void StopGameClbk()
        {
            SendReturnToLobby();
            ChangeTo(lobbyPanel);
        }

        //===================

        public override void OnStartHost()
        {
            base.OnStartHost();

            ChangeTo(lobbyPanel);
            backDelegate = StopHostClbk;
            SetServerInfo("Hosting", networkAddress);
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            infoPanel.gameObject.SetActive(false);

            if (!NetworkServer.active)
            {//only to do on pure client (not self hosting client)
                ChangeTo(lobbyPanel);
                backDelegate = StopClientClbk;
                SetServerInfo("Client", networkAddress);
            }
        }

        public override void OnMatchCreate(UnityEngine.Networking.Match.CreateMatchResponse matchInfo)
        {
            base.OnMatchCreate(matchInfo);

            _currentMatchID = (System.UInt64)matchInfo.networkId;
        }

        public void OnMatchDestroyed(BasicResponse resp)
        {
            if (_disconnectServer)
            {
                StopMatchMaker();
                StopHost();
            }
        }

        // ----------------- Server callbacks ------------------

        //we want to disable the button JOIN if we don't have enough player
        //But OnLobbyClientConnect isn't called on hosting player. So we override the lobbyPlayer creation
        public override GameObject OnLobbyServerCreateLobbyPlayer(NetworkConnection conn, short playerControllerId)
        {
            GameObject obj = Instantiate(lobbyPlayerPrefab.gameObject) as GameObject;

            LobbyPlayer newPlayer = obj.GetComponent<LobbyPlayer>();
            newPlayer.RpcToggleJoinButton(numPlayers + 1 >= minPlayers); ;

            for (int i = 0; i < numPlayers; ++i)
            {
                LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

                if (p != null)
                {
                    p.RpcToggleJoinButton(numPlayers + 1 >= minPlayers);
                }
            }

            return obj;
        }

        public override void OnLobbyServerDisconnect(NetworkConnection conn)
        {
            for (int i = 0; i < numPlayers; ++i)
            {
                LobbyPlayer p = lobbySlots[i] as LobbyPlayer;

                if (p != null)
                {
                    p.RpcToggleJoinButton(numPlayers >= minPlayers);
                }
            }

        }

        public override bool OnLobbyServerSceneLoadedForPlayer(GameObject lobbyPlayer, GameObject gamePlayer)
        {
            //This hook allows you to apply state data from the lobby-player to the game-player
            //just subclass "LobbyHook" and add it to the lobby object.

            if (_lobbyHooks)
                _lobbyHooks.OnLobbyServerSceneLoadedForPlayer(this, lobbyPlayer, gamePlayer);

            return true;
        }


        // --- Countdown management

        static protected float _matchStartCountdown = 5.0f;

        public override void OnLobbyServerPlayersReady()
        {
            int num = 0;
            foreach (NetworkConnection conn in NetworkServer.connections)
            {
                if (conn != null)
                    num += CheckConnectionIsReadyToBegin(conn);
            }
            if (this.minPlayers > 0 && num < NetworkServer.connections.Count)
                return;

            StartCoroutine(ServerCountdownCoroutine());
        }

        private int CheckConnectionIsReadyToBegin(NetworkConnection conn)
        {
            int num = 0;
            using (List<PlayerController>.Enumerator enumerator = conn.playerControllers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    PlayerController current = enumerator.Current;
                    if (current.IsValid && current.gameObject.GetComponent<NetworkLobbyPlayer>().readyToBegin)
                        ++num;
                }
            }
            return num;
        }

        public IEnumerator ServerCountdownCoroutine()
        {
            float remainingTime = _matchStartCountdown;
            int floorTime = Mathf.FloorToInt(remainingTime);

            while(remainingTime > 0)
            {
                yield return null;

                remainingTime -= Time.deltaTime;
                int newFloorTime = Mathf.FloorToInt(remainingTime);

                if(newFloorTime != floorTime)
                {//to avoid flooding the network of message, we only send a notice to cleint when the number of second do change.
                    floorTime = newFloorTime;

                    for (int i = 0; i < lobbySlots.Length; ++i)
                    {
                        if (lobbySlots[i] != null)
                        {//there is max player slot, so some could be == null, need to test it ebfore accessing!
                            (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(floorTime);
                        }
                    }
                }
            }

            for (int i = 0; i < lobbySlots.Length; ++i)
            {
                if (lobbySlots[i] != null)
                {
                    (lobbySlots[i] as LobbyPlayer).RpcUpdateCountdown(0);
                }
            }

            ServerChangeScene(playScene);
        }



        // ----------------- Client callbacks ------------------

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);
            ChangeTo(mainMenuPanel);
        }

        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            ChangeTo(mainMenuPanel);
            infoPanel.Display("Cient error : " + (errorCode == 6 ? "timeout" : errorCode.ToString()), "Close", null);
        }

        public void OnMatchJoined(JoinMatchResponse matchInfo)
        {
            if (LogFilter.logDebug)
                Debug.Log((object)"NetworkManager OnMatchJoined ");
            if (matchInfo.success)
            {
                if(UnityEngine.Networking.Utility.GetAccessTokenForNetwork(matchInfo.networkId) == null)
                    UnityEngine.Networking.Utility.SetAccessTokenForNetwork(matchInfo.networkId, new NetworkAccessToken(matchInfo.accessTokenString));
                this.StartClient(new MatchInfo(matchInfo));
            }
            else
            {
                if (!LogFilter.logError)
                    return;
                Debug.LogError((object)("Join Failed:" + (object)matchInfo));
            }
        }
    }
}
