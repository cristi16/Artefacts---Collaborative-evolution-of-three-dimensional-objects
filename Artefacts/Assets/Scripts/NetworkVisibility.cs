using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

// Attach this to objects that need their visibility updated as the player moves around
[RequireComponent(typeof(NetworkIdentity))]
public class NetworkVisibility : NetworkBehaviour
{

    public List<NetworkConnection> playersObserving = new List<NetworkConnection>();
    public NetworkIdentity networkIdentity;

    void Awake()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
    {
        foreach (NetworkConnection net in playersObserving)
        {
            observers.Add(net);
        }
        return true;
    }

    public override bool OnCheckObserver(NetworkConnection newObserver)
    {
        return false;
    }

    // called hiding and showing objects on the host
    public override void OnSetLocalVisibility(bool vis)
    {
        SetVis(gameObject, vis);
    }

    static void SetVis(GameObject go, bool vis)
    {
        foreach (var r in go.GetComponents<Renderer>())
        {
            r.enabled = vis;
        }
        for (int i = 0; i < go.transform.childCount; i++)
        {
            var t = go.transform.GetChild(i);
            SetVis(t.gameObject, vis);
        }
    }
}