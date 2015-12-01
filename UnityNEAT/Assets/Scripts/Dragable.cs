using System;
using System.Collections;
using HighlightingSystem;
using UnityEngine;

public class Dragable : MonoBehaviour
{
    public const float k_DragDistance = 5f;
    public bool IsDragging = false;

    private float oldDrag;
    private float oldAngularDrag;

    public void StopDragging()
    {
        IsDragging = false;
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.drag = oldDrag;
        rb.angularDrag = oldAngularDrag;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        GetComponent<Highlighter>().ConstantOff();
    }

    public void StartDragging()
    {
        IsDragging = true;
        StartCoroutine(DragObject(k_DragDistance));
    }

    private IEnumerator DragObject(float distance)
    {
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        oldDrag = rb.drag;
        oldAngularDrag = rb.angularDrag;
        rb.drag = 20f;

        while (IsDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            var desiredPosition = ray.GetPoint(distance);
            //if(desiredPosition.y < 0f)
            //    desiredPosition = new Vector3(desiredPosition.x, 0f, desiredPosition.z);
            //m_SpringJoint.transform.position = desiredPosition;
            var force = (desiredPosition - transform.position) * 1000;
            GetComponent<Rigidbody>().AddForce(force);
            yield return null;
        }
    }
}

