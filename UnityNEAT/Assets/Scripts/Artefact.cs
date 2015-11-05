using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.Genomes.Neat;
using SharpNeat.Phenomes;
using UnityEngine.Networking;

public class Artefact : NetworkBehaviour
{ 
    [HideInInspector, SyncVar] public string SerializedGenome;

    private static VoxelVolume m_voxelVolume = new VoxelVolume() { width = 16, height = 16, length = 16 };

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (SerializedGenome == String.Empty)
        {
            Debug.LogError("Spawned artefact without genome!");
        }

        // Deserialize genome
        var genome = NeatGenomeXmlIO.ReadGenome(XmlReader.Create(new StringReader(SerializedGenome)), true);
        // we need to assign genome factory we used for creating the genome
        genome.GenomeFactory = EvolutionHelper.CreateGenomeFactory();

        // Decode phenome from genome
        var genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());
        var phenome = genomeDecoder.Decode(genome);

        // Evaluate phenome using voxel grid and generate mesh using Marching Cubes algorithm
        var mesh = GenerateMesh(phenome);

        // Add required components in order to render mesh
        DisplayMesh(mesh);
    }

    private Mesh GenerateMesh(IBlackBox phenome)
    {
        var voxels = new float[m_voxelVolume.width, m_voxelVolume.height, m_voxelVolume.length];
        var meshFillOutput = new float[m_voxelVolume.width, m_voxelVolume.height, m_voxelVolume.length];

        var minFill = 1f;
        var maxFill = -1f;

        for (int x = 0; x < m_voxelVolume.width; x++)
        {
            for (int y = 0; y < m_voxelVolume.height; y++)
            {
                for (int z = 0; z < m_voxelVolume.length; z++)
                {
                    ISignalArray inputArr = phenome.InputSignalArray;
                    inputArr[0] = Mathf.Abs((float)x / (m_voxelVolume.width - 1) * 2 - 1);
                    inputArr[1] = Mathf.Abs((float)y / (m_voxelVolume.height - 1) * 2 - 1);
                    inputArr[2] = Mathf.Abs((float)z / (m_voxelVolume.length - 1) * 2 - 1);

                    inputArr[3] = DistanceFunctions.DistanceToCenter(x, y, z, m_voxelVolume);

                    phenome.Activate();

                    ISignalArray outputArr = phenome.OutputSignalArray;

                    voxels[x, y, z] = (float)outputArr[0];

                    meshFillOutput[x, y, z] = (float)outputArr[0];

                    if (voxels[x, y, z] < minFill)
                        minFill = voxels[x, y, z];
                    if (voxels[x, y, z] > maxFill)
                        maxFill = voxels[x, y, z];

                    if (x == 0 || x == m_voxelVolume.width - 1 || y == 0 || y == m_voxelVolume.height - 1 || z == 0 || z == m_voxelVolume.length - 1)
                        voxels[x, y, z] = -1f;
                }
            }
        }

        Debug.Log("Max fill: " + maxFill);
        Debug.Log("Min fill: " + minFill);

        // TODO: need to find a workaround for this. It results in cubes. In multiplayer we don't want to spawn cubes as seeds.
        // Either save network to disk, look and see what's wrong or maybe if this happens on the client it could request another regeneration.
        if (Mathf.Approximately(minFill, maxFill))
        {
            Debug.LogError("Min equals max");
        }

        //MarchingCubes.SetTarget(minFill + (maxFill - minFill) /2f);

        for (int index00 = 1; index00 < voxels.GetLength(0) - 1; index00++)
            for (int index01 = 1; index01 < voxels.GetLength(1) - 1; index01++)
                for (int index02 = 1; index02 < voxels.GetLength(2) - 1; index02++)
                {
                    if (Mathf.Approximately(meshFillOutput[0, 0, 0], maxFill))
                        voxels[index00, index01, index02] = minFill + (maxFill - voxels[index00, index01, index02]);

                    voxels[index00, index01, index02] = voxels[index00, index01, index02] < minFill + (maxFill - minFill) / 2f ? 0f : 1f;
                }

        return MarchingCubes.CreateMesh(voxels);
    }

    void DisplayMesh(Mesh mesh)
    {
        mesh.RecalculateNormals();
        mesh.uv = new Vector2[mesh.vertices.Length];

        gameObject.AddComponent<MeshFilter>().mesh = mesh;
        gameObject.AddComponent<MeshRenderer>();
        gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        gameObject.AddComponent<ProceduralMesh>();
    }
}
