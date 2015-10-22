using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerNetworkSetup : NetworkBehaviour
{
    public GameObject go;

    public override void OnStartLocalPlayer()
    {
        GetComponent<CharacterController>().enabled = true;
        GetComponent<FirstPersonController>().enabled = true;
        GetComponentInChildren<Camera>().enabled = true;
        GetComponentInChildren<AudioListener>().enabled = true;
    }

    void Update()
    {
        if (isLocalPlayer)
        {
            if(Input.GetKeyDown(KeyCode.R))
                CmdSpawnSphere();
            if (Input.GetKeyDown(KeyCode.F))
                GetComponent<FirstPersonController>().Freeze = !GetComponent<FirstPersonController>().Freeze;
        }
    }

    [Command]
    public void CmdSpawnSphere()
    {
        var instance = Instantiate(go);
        NetworkServer.SpawnWithClientAuthority(instance, connectionToClient);
    }
}
