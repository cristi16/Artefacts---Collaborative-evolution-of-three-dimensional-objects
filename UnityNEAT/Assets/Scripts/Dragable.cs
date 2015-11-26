using System;
using System.Collections;
using UnityEngine;

public class Dragable : MonoBehaviour
{
    public const float k_DragDistance = 5f;
    public bool IsDragging = false;

    const float k_Spring = 50.0f;
    const float k_Damper = 5.0f;
    const float k_Drag = 10.0f;
    const float k_AngularDrag = 5.0f;
    const float k_Distance = 0.2f;
    const bool k_AttachToCenterOfMass = false;

    private SpringJoint m_SpringJoint;


    public void StopDragging()
    {
        IsDragging = false;
    }

    public void StartDragging()
    {
        if (!m_SpringJoint)
        {
            var go = new GameObject("Rigidbody dragger");
            Rigidbody body = go.AddComponent<Rigidbody>();
            m_SpringJoint = go.AddComponent<SpringJoint>();
            body.isKinematic = true;
        }

        m_SpringJoint.transform.position = transform.position;
        m_SpringJoint.anchor = Vector3.zero;

        m_SpringJoint.spring = k_Spring;
        m_SpringJoint.damper = k_Damper;
        m_SpringJoint.maxDistance = k_Distance;
        m_SpringJoint.connectedBody = GetComponent<Rigidbody>();

        IsDragging = true;
        StartCoroutine(DragObject(k_DragDistance));
    }

    private IEnumerator DragObject(float distance)
    {
        var oldDrag = m_SpringJoint.connectedBody.drag;
        var oldAngularDrag = m_SpringJoint.connectedBody.angularDrag;
        m_SpringJoint.connectedBody.drag = k_Drag;
        m_SpringJoint.connectedBody.angularDrag = k_AngularDrag;

        while (IsDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            m_SpringJoint.transform.position = ray.GetPoint(distance);
            yield return null;
        }
        if (m_SpringJoint.connectedBody)
        {
            m_SpringJoint.connectedBody.drag = oldDrag;
            m_SpringJoint.connectedBody.angularDrag = oldAngularDrag;
            m_SpringJoint.connectedBody = null;
        }
    }
}

