using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

// Attach this to player prefab
[RequireComponent(typeof(SphereCollider))]
public class NetworkPlayerVisibility : NetworkBehaviour
{

    [SerializeField]
    private int visRadius = 50; // Radius of sphere collider
    [SerializeField]
    private float visUpdateInterval = 2000; // Update time in ms
    private SphereCollider collider;
    private List<NetworkVisibility> changedObjects = new List<NetworkVisibility>(); // Objects that have changed visibility
    private float visUpdateTime;

    void Awake()
    {
        collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = visRadius;
    }

    void Update()
    {
        if (!NetworkServer.active)
            return;

        if (Time.time - visUpdateTime > visUpdateInterval)
        {
            RebuildChangedObjects();
            visUpdateTime = Time.time;
        }
    }

    void RebuildChangedObjects()
    {
        foreach (NetworkVisibility net in changedObjects)
        {
            net.networkIdentity.RebuildObservers(false);
        }
        changedObjects.Clear();
    }

    void OnTriggerEnter(Collider col)
    {
        NetworkVisibility net = col.GetComponent<NetworkVisibility>();
        if (net != null && connectionToClient != null)
        {
            net.playersObserving.Add(connectionToClient);
            changedObjects.Add(net);
        }
    }

    void OnTriggerExit(Collider col)
    {
        NetworkVisibility net = col.GetComponent<NetworkVisibility>();
        if (net != null && connectionToClient != null)
        {
            net.playersObserving.Remove(connectionToClient);
            changedObjects.Add(net);
        }
    }

    // Use these to update radius and interval in game
    public void SetVisualRadius(int radius)
    {
        visRadius = radius;
        collider.radius = radius;
    }

    public void SetUpdateInterval(float interval)
    {
        visUpdateInterval = interval;
    }

}
