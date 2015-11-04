using UnityEngine;
using System.Collections;
using System.Text;
using UnityEngine.Networking;

public class DummySphere : NetworkBehaviour
{
    [SyncVar]
    public string dummyString;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    void Update()
    {
        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.J))
                CmdJump();
        }
    }

    [Command]
    public void CmdJump()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 30000; i++)
            sb.Append("a");
        Debug.Log("string size : " + System.Text.ASCIIEncoding.ASCII.GetByteCount(sb.ToString()));
        RpcMessage(sb.ToString());
    }

    [ClientRpc]
    public void RpcMessage(string message)
    {
        Debug.Log("rpc message -> isServer: " + isServer);
        GetComponent<Rigidbody>().AddForce(Vector3.up * 5f, ForceMode.Impulse);
    }

}
