using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Artefact : NetworkBehaviour
{
    [SyncVar]
    public string SerializedGenome;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Deserialize genome

        // Decode phenome from genome

        // Evaluate phenome using voxel grid

        // Apply marching cubes to volumetric data and generate mesh
    }
}
