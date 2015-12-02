using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class CustomNetworkManager : NetworkManager
{
    public bool hostServer;
    void Start()
    {
        if (hostServer)
        {
            string localIP = "localhost";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }
            networkAddress = localIP;
        }
    }

    //public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    //{
    //    Transform startPosition = this.GetStartPosition();
    //    GameObject player;
    //    if(startPosition == null)
    //        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
    //    else 
    //        player = Instantiate(playerPrefab, startPosition.position, startPosition.rotation) as GameObject;

    //    NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    //}
}
