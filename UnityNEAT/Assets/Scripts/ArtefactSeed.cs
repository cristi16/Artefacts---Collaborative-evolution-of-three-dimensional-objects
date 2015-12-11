using UnityEngine;
using UnityEngine.Networking;

public class ArtefactSeed : Artefact
{
    public float spawnJumpForce = 30f;

    [HideInInspector] public bool IsInInventory;

    [HideInInspector]
    public Vector3 facingDirection;

    public override void OnStartServer()
    {
        base.OnStartServer();
        GetComponent<Rigidbody>().AddForce(facingDirection * spawnJumpForce, ForceMode.Impulse);
    }
}
