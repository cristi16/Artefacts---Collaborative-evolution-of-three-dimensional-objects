using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DraggingHelper : NetworkBehaviour
{

    private Rigidbody draggedBody;

    [Command]
    public void CmdSetDraggedBody(NetworkInstanceId netId)
    {
        draggedBody = NetworkServer.FindLocalObject(netId).GetComponent<Rigidbody>();
        RpcSetDraggedBody(netId);
    }

    [ClientRpc]
    void RpcSetDraggedBody(NetworkInstanceId netId)
    {
        draggedBody = ClientScene.FindLocalObject(netId).GetComponent<Rigidbody>();
    }

    [Command]
    public void CmdSetVelocity(Vector3 velocity)
    {
        draggedBody.velocity = velocity;
    }

    [Command]
    public void CmdAddForce(Vector3 force)
    {
        draggedBody.AddForce(force);
    }

    [Command]
    public void CmdAddTorque(Vector3 torque)
    {
        //draggedBody.AddTorque(torque);
        draggedBody.transform.RotateAround(draggedBody.transform.position, torque, 100f * Time.deltaTime);
    }

    [Command]
    public void CmdStartDragging(NetworkInstanceId netId)
    {
        StartDragging(netId);
        RpcStartDragging(netId);
    }

    [ClientRpc]
    void RpcStartDragging(NetworkInstanceId netId)
    {
        StartDragging(netId);
    }

    [Command]
    public void CmdStopDragging()
    {
        StopDragging();
        RpcStopDragging();
    }

    [ClientRpc]
    void RpcStopDragging()
    {
        StopDragging();
    }

    void StartDragging(NetworkInstanceId netId)
    {
        draggedBody = ClientScene.FindLocalObject(netId).GetComponent<Rigidbody>();

        var dragable = draggedBody.GetComponent<Dragable>();
        dragable.IsDragging = true;
        if (dragable.IsAttached)
            dragable.DetachJoints();

        draggedBody.isKinematic = false;
        draggedBody.useGravity = false;
        draggedBody.drag = 12f;
    }

    void StopDragging()
    {
        var dragable = draggedBody.GetComponent<Dragable>();
        dragable.IsDragging = false;
        dragable.AddJoints();

        draggedBody.velocity = Vector3.zero;
        draggedBody.angularVelocity = Vector3.zero;

        draggedBody.isKinematic = true;
        draggedBody.useGravity = true;
        draggedBody.drag = 1f;
        draggedBody.angularDrag = 5f;
    }
}
