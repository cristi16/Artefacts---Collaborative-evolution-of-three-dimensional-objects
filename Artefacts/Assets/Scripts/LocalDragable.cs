using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HighlightingSystem;
using UnityEngine;
using UnityEngine.Networking;

public class LocalDragable : MonoBehaviour
{
    public const float k_DragDistance = 5f;
    public bool IsDragging = false;

    public Transform playerTransform;

    private Rigidbody body;
    public List<ContactPoint> contactPoints = new List<ContactPoint>();

    void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public void StopDragging()
    {
        IsDragging = false;
        Stop();
        GetComponent<Highlighter>().ConstantOff();

        foreach (var contactPoint in contactPoints)
        {
            contactPoint.otherCollider.attachedRigidbody.isKinematic = false;
        }
        contactPoints.Clear();
    }

    public void StartDragging()
    {
        body = GetComponent<Rigidbody>();

        IsDragging = true;
        Initialize();
        GetComponent<Highlighter>().ConstantOn(Color.white);
        StartCoroutine(DragObject(k_DragDistance));
    }

    private IEnumerator DragObject(float distance)
    {

        while (IsDragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            var desiredPosition = ray.GetPoint(distance);
            var force = (desiredPosition - transform.position) * 1000;

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
                    body.transform.RotateAround(body.transform.position, playerTransform.right, 100f * Time.deltaTime);
                    //body.AddTorque(playerTransform.right * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.S))
                {
                    body.transform.RotateAround(body.transform.position, -playerTransform.right, 100f * Time.deltaTime);
                    //body.AddTorque(-playerTransform.right * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.A))
                {
                    body.transform.RotateAround(body.transform.position, playerTransform.up, 100f * Time.deltaTime);
                   // body.AddTorque(playerTransform.up * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.D))
                {
                    body.transform.RotateAround(body.transform.position, -playerTransform.up, 100f * Time.deltaTime);
                    //body.AddTorque(-playerTransform.up * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.Q))
                {
                    body.transform.RotateAround(body.transform.position, playerTransform.forward, 100f * Time.deltaTime);
                    //body.AddTorque(playerTransform.forward * 5000 * Time.deltaTime);
                }

                if (Input.GetKey(KeyCode.E))
                {
                    body.transform.RotateAround(body.transform.position, -playerTransform.forward, 100f * Time.deltaTime);
                    //body.AddTorque(-playerTransform.forward * 5000 * Time.deltaTime);
                }

                var scrollInput = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scrollInput) >= 0.1f)
                    transform.position = transform.position + ray.direction * Math.Sign(scrollInput) * 0.5f;
            }
            else
            {
                body.angularDrag = 100;
                transform.position = desiredPosition;
            }

            yield return null;
        }
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

        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
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
}