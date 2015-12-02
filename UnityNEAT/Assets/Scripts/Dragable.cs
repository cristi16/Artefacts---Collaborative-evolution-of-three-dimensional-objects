using System;
using System.Collections;
using HighlightingSystem;
using UnityEngine;
using UnityEngine.Networking;

public class Dragable : NetworkBehaviour
{
    public const float k_DragDistance = 5f;
    [SyncVar]
    public bool IsDragging = false;

    private float oldDrag;
    private float oldAngularDrag;
    private Rigidbody body;


    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        if(IsDragging)
            CmdStartDragging();

    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
    }

    public void StopDragging()
    {
        IsDragging = false;
        CmdStopDragging();
        GetComponent<Highlighter>().ConstantOff();
    }

    public void StartDragging()
    {
        IsDragging = true;
        StartCoroutine(DragObject(k_DragDistance));
    }

    private IEnumerator DragObject(float distance)
    {

        while (IsDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            var desiredPosition = ray.GetPoint(distance);
            var force = (desiredPosition - transform.position) * 1000;
            body.AddForce(force);
            yield return null;
        }
    }

    [Command]
    void CmdStartDragging()
    {
        IsDragging = true;
        RpcInitialize();
    }

    [Command]
    void CmdStopDragging()
    {
        IsDragging = false;
        RpcStop();
    }

    [ClientRpc]
    void RpcInitialize()
    {
        oldDrag = body.drag;
        oldAngularDrag = body.angularDrag;
        body.useGravity = false;
        body.drag = 20f;
    }
    [ClientRpc]
    void RpcStop()
    {
        body.useGravity = true;
        body.drag = oldDrag;
        body.angularDrag = oldAngularDrag;

        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}

