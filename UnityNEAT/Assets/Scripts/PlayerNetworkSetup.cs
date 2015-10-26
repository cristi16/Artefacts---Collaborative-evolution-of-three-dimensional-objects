using UnityEngine;
using System.Collections;
using System.Text;
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
            if (Input.GetKeyDown(KeyCode.R))
                CmdSpawnSphere();
            if (Input.GetKeyDown(KeyCode.F))
                GetComponent<FirstPersonController>().IsFrozen = !GetComponent<FirstPersonController>().IsFrozen;
        }
    }

    [Command]
    public void CmdSpawnSphere()
    {
        var instance = Instantiate(go);

        var sb = new StringBuilder();
        for (int i = 0; i < 30000; i++)
            sb.Append("a");

        instance.GetComponent<DummySphere>().dummyString = sb.ToString();
        NetworkServer.Spawn(instance);
    }
}
