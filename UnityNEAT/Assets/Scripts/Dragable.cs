using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HighlightingSystem;
using UnityEngine;
using UnityEngine.Networking;

public class Dragable : NetworkBehaviour
{
    public const float k_DragDistance = 5f;
    [SyncVar]
    public bool IsDragging = false;

    [SyncVar] public bool IsAttached = false;

    public Transform playerTransform;

    private Rigidbody body;
    private List<ContactPoint> contactPoints = new List<ContactPoint>();
    // colliders for which we have a fixed joint component
    public List<Collider> collidersWeAttachTo = new List<Collider>();
    // colliders that have a fixed joint component with us
    public List<Collider> collidersAttachingToUs = new List<Collider>(); 

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        if (IsDragging)
        {
            CmdStartDragging();
            if(IsAttached)
                Detach();
        }  
    }

    public override void OnStopAuthority()
    {
        base.OnStopAuthority();
    }

    public void StopDragging()
    {
        IsDragging = false;
        Stop();
        CmdStopDragging();
        GetComponent<Highlighter>().ConstantOff();

        if(contactPoints.Count > 0)
            CmdSetAttached();

        foreach (var contactPoint in contactPoints)
        {
            CmdAddJoint(contactPoint.otherCollider.GetComponent<NetworkIdentity>().netId);
        }

        contactPoints.Clear();
    }

    public void StartDragging()
    {
        body = GetComponent<Rigidbody>();

        IsDragging = true;
        Initialize();
        StartCoroutine(DragObject(k_DragDistance));
    }

    private IEnumerator DragObject(float distance)
    {
        while (IsDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            var desiredPosition = ray.GetPoint(distance);
            var force = (desiredPosition - transform.position) * 1000;

            if (Input.GetMouseButton(1))
            {
                body.angularDrag = 5f;
                if (Input.GetKey(KeyCode.W))
                {
                    body.AddTorque(playerTransform.right * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    body.AddTorque(-playerTransform.right * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.A))
                {
                    body.AddTorque(playerTransform.up * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    body.AddTorque(-playerTransform.up * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    body.AddTorque(playerTransform.forward * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.E))
                {
                    body.AddTorque(-playerTransform.forward * 5000 * Time.deltaTime);
                }
            }
            else
            {
                body.angularDrag = 100f;
                body.AddForce(force);
            }

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

    [Command]
    public void CmdSetAttached()
    {
        IsAttached = true;
    }

    [Command]
    public void CmdDetach()
    {
        IsAttached = false;
        RpcDetachJoints();
    }

    [ClientRpc]
    void RpcInitialize()
    {
        Initialize();
    }
    [ClientRpc]
    void RpcStop()
    {
        Stop();
    }

    void Initialize()
    {
        body.useGravity = false;
        body.drag = 12f;
    }

    void Stop()
    {
        body.useGravity = true;
        body.drag = 1f;
        body.angularDrag = 5f;

        //body.velocity = Vector3.zero;
        //body.angularVelocity = Vector3.zero;
    }

    [Command]
    void CmdAddJoint(NetworkInstanceId connectedBodyId)
    {
        RpcAddJoint(connectedBodyId);
    }

    [ClientRpc]
    void RpcAddJoint(NetworkInstanceId connectedBodyId)
    {
        var connectedGo = ClientScene.FindLocalObject(connectedBodyId);

        var rb = connectedGo.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        AddJoint(rb);
    }

    void AddJoint(Rigidbody rb)
    {
        if (isServer)
        {
            var fixedJoint = gameObject.AddComponent<FixedJoint>();
            fixedJoint.enableCollision = true;
            fixedJoint.connectedBody = rb;
        }
        if (isServer)
            rb.GetComponent<Dragable>().IsAttached = true;
        collidersWeAttachTo.Add(rb.GetComponent<Collider>());
        rb.GetComponent<Dragable>().collidersAttachingToUs.Add(GetComponent<Collider>());
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsDragging && LayerMask.LayerToName(collision.gameObject.layer) == "Artefact")
        {
            if(contactPoints.Any(x => x.otherCollider == collision.collider)) return;

            contactPoints.Add(collision.contacts[0]);
            collision.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (IsDragging && LayerMask.LayerToName(collision.gameObject.layer) == "Artefact")
        {
            contactPoints.RemoveAll(x => x.otherCollider == collision.collider);
            collision.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    public void Detach()
    {
        CmdDetach();
    }

    [ClientRpc]
    void RpcDetachJoints()
    {
        DetachJoints();
    }

    void DetachJoints()
    {
        foreach (var col in collidersAttachingToUs)
        {
            var dragable = col.GetComponent<Dragable>();
            dragable.RemoveAttachedCollider(GetComponent<Collider>());
            dragable.CheckIfStillAttached();
        }

        collidersAttachingToUs.Clear();

        foreach (var col in collidersWeAttachTo)
        {
            var dragable = col.GetComponent<Dragable>();
            dragable.collidersAttachingToUs.Remove(GetComponent<Collider>());
            dragable.CheckIfStillAttached();
        }

        collidersWeAttachTo.Clear();

        foreach (var joint in GetComponents<FixedJoint>())
        {
            Destroy(joint);
        }
    }

    void RemoveAttachedCollider(Collider collider)
    {
        collidersWeAttachTo.Remove(collider);
        foreach (var joint in GetComponents<FixedJoint>())
        {
            if (joint.connectedBody.GetComponent<Collider>() == collider)
                Destroy(joint);
        }
    }

    public void CheckIfStillAttached()
    {
        if (isServer)
        {
            if (collidersWeAttachTo.Count == 0 && collidersAttachingToUs.Count == 0)
                IsAttached = false;
        }
    }
}

