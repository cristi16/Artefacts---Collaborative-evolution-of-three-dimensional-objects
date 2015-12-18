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

    [SyncVar]
    public bool IsAttached;

    public Transform playerTransform;
    //public DraggingHelper draggingHelper;

    private Rigidbody body;
    public List<ContactPoint> contactPoints = new List<ContactPoint>();
    // colliders for which we have a fixed joint component
    public List<Collider> collidersWeAttachTo = new List<Collider>();
    // colliders that have a fixed joint component with us
    public List<Collider> collidersAttachingToUs = new List<Collider>();

    private Highlighter highlighter;

    void Start()
    {
        body = GetComponent<Rigidbody>();
        highlighter = GetComponent<Highlighter>();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();
        if (IsDragging)
        {
            CmdSetDragging(true);
        }
    }

    public void StartDragging()
    {
        //contactPoints.Clear();

        body = GetComponent<Rigidbody>();
        //draggingHelper = playerTransform.GetComponent<DraggingHelper>();

        IsDragging = true;
        body.isKinematic = false;
        //draggingHelper.CmdStartDragging(GetComponent<NetworkIdentity>().netId);
        GetComponent<Highlighter>().ConstantOn(Color.white);

        StartCoroutine(DragObject(k_DragDistance));
    }

    public void StopDragging()
    {
        IsDragging = false;
        body.isKinematic = true;
        CmdSetDragging(false);
        GetComponent<Highlighter>().ConstantOff();
    }

    public void StopDragging(NetworkInstanceId netId)
    {
        //draggingHelper = playerTransform.GetComponent<DraggingHelper>();

        //draggingHelper.CmdSetDraggedBody(netId);
        StopDragging();
    }

    private IEnumerator DragObject(float distance)
    {
        while (IsDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            var desiredPosition = ray.GetPoint(distance);

            Debug.DrawLine(ray.origin, ray.origin + ray.direction * distance);

            if (Input.GetMouseButtonUp(1))
            {
                distance = Vector3.Distance(ray.origin, transform.position);
                desiredPosition = ray.GetPoint(distance);
            }

            if (Input.GetMouseButton(1))
            {
                body.angularDrag = 5f;
                if (Input.GetKey(KeyCode.W))
                {
                    AddTorque(playerTransform.right);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    AddTorque(-playerTransform.right);
                }

                if (Input.GetKey(KeyCode.A))
                {
                    AddTorque(playerTransform.up);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    AddTorque(-playerTransform.up);
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    AddTorque(playerTransform.forward);
                }

                if (Input.GetKey(KeyCode.E))
                {
                    AddTorque(-playerTransform.forward);
                }

                var scrollInput = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scrollInput) >= 0.1f)
                    transform.position = transform.position + ray.direction * Math.Sign(scrollInput) * 0.5f;
            }
            else
            {
                body.angularDrag = 100f;
                transform.position = desiredPosition;
                //transform.LookAt(ray.origin);
            }

            highlighter.On(contactPoints.Count > 0 ? Color.yellow : Color.white);

            yield return null;
        }
    }

    [Command]
    private void CmdSetDragging(bool value)
    {
        IsDragging = value;
        RpcSetDragging(value);
    }

    [ClientRpc]
    private void RpcSetDragging(bool value)
    {
        GetComponent<Rigidbody>().isKinematic = !value;
        GetComponent<Collider>().enabled = !value;
    }

    void AddTorque(Vector3 axis)
    {
        //if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(1))
        //{
        //    transform.RotateAround(transform.position, axis, 15);
        //    transform.eulerAngles = new Vector3(((int)transform.eulerAngles.x / 15) * 15, ((int)transform.eulerAngles.y / 15) * 15, ((int)transform.eulerAngles.z / 15) * 15);
        //}
        //else
            transform.RotateAround(transform.position, axis, 100f * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsDragging && LayerMask.LayerToName(collision.gameObject.layer) == "Artefact")
        {
            if (contactPoints.Any(x => x.otherCollider == collision.collider)) return;

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

