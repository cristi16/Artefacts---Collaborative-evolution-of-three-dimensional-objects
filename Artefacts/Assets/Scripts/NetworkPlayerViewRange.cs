using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

// Attach this to player prefab
[RequireComponent(typeof(SphereCollider))]
public class NetworkPlayerViewRange : NetworkBehaviour
{
    [SerializeField]
    private int visRadius = 50; // Radius of sphere collider
    private SphereCollider collider;

    void Awake()
    {
        collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = visRadius;
    }

    void OnTriggerEnter(Collider col)
    {
        if(isLocalPlayer == false) return;

        if (col.gameObject.layer == LayerMask.NameToLayer("Seed") || col.gameObject.layer == LayerMask.NameToLayer("Artefact"))
        {
            col.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    void OnTriggerExit(Collider col)
    {
        if(isLocalPlayer == false) return;

        if (col.gameObject.layer == LayerMask.NameToLayer("Seed") || col.gameObject.layer == LayerMask.NameToLayer("Artefact"))
        {
            col.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    // Use these to update radius and interval in game
    public void SetVisualRadius(int radius)
    {
        visRadius = radius;
        collider.radius = radius;
    }
}
