using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class DraggingHelper : NetworkBehaviour
{

    private Rigidbody draggedBody;

    [Command]
    public void CmdSetVelocity(Vector3 velocity)
    {
        draggedBody.velocity = velocity;
    }

    [Command]
    public void CmdAddTorque(Vector3 torque)
    {
        draggedBody.AddTorque(torque);
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

        draggedBody.useGravity = true;
        draggedBody.drag = 1f;
        draggedBody.angularDrag = 5f;
    }
}
