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

    [SyncVar] public bool IsAttached;

    public Transform playerTransform;
    public DraggingHelper draggingHelper;

    private Rigidbody body;
    public List<ContactPoint> contactPoints = new List<ContactPoint>();
    // colliders for which we have a fixed joint component
    public List<Collider> collidersWeAttachTo = new List<Collider>();
    // colliders that have a fixed joint component with us
    public List<Collider> collidersAttachingToUs = new List<Collider>(); 

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public void StartDragging()
    {
        //contactPoints.Clear();

        body = GetComponent<Rigidbody>();
        draggingHelper = playerTransform.GetComponent<DraggingHelper>();

        IsDragging = true;
        draggingHelper.CmdStartDragging(GetComponent<NetworkIdentity>().netId);
        GetComponent<Highlighter>().ConstantOn(Color.white);

        StartCoroutine(DragObject(k_DragDistance));
    }

    public void StopDragging()
    {
        IsDragging = false;
        draggingHelper.CmdStopDragging();
        GetComponent<Highlighter>().ConstantOff();
    }

    public void StopDragging(NetworkInstanceId netId)
    {
        draggingHelper = playerTransform.GetComponent<DraggingHelper>();

        draggingHelper.CmdSetDraggedBody(netId);
        StopDragging();
    }

    private IEnumerator DragObject(float distance)
    {
        while (IsDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            var desiredPosition = ray.GetPoint(distance);
            var force = (desiredPosition - transform.position) * 5000;
            var velocity = (force / body.mass) * Time.fixedDeltaTime;

            if (Input.GetMouseButton(1))
            {
                body.angularDrag = 5f;
                if (Input.GetKey(KeyCode.W))
                {
                    draggingHelper.CmdAddTorque(playerTransform.right * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    draggingHelper.CmdAddTorque(-playerTransform.right * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.A))
                {
                    draggingHelper.CmdAddTorque(playerTransform.up * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    draggingHelper.CmdAddTorque(-playerTransform.up * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    draggingHelper.CmdAddTorque(playerTransform.forward * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.E))
                {
                    draggingHelper.CmdAddTorque(-playerTransform.forward * 5000 * Time.deltaTime);
                }
            }
            else
            {
                body.angularDrag = 100f;
                draggingHelper.CmdSetVelocity(velocity);
            }

            yield return null;
        }
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

    public void AddJoints()
    {
        if (contactPoints.Count > 0)
            IsAttached = true;

        foreach (var contactPoint in contactPoints)
        {
            var rb = contactPoint.otherCollider.GetComponent<Rigidbody>();
            rb.isKinematic = false;

            var fixedJoint = gameObject.AddComponent<FixedJoint>();
            fixedJoint.enableCollision = true;
            fixedJoint.connectedBody = rb;

            if (isServer)
                rb.GetComponent<Dragable>().IsAttached = true;

            collidersWeAttachTo.Add(rb.GetComponent<Collider>());
            rb.GetComponent<Dragable>().collidersAttachingToUs.Add(GetComponent<Collider>());
        }

        contactPoints.Clear();
    }

    public void DetachJoints()
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

