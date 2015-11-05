using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;

public class SelectiveMeshEvolver : MonoBehaviour
{
    public ArtefactEvaluator.VoxelVolume m_voxelVolume;
    public ArtefactEvaluator.InputType InputType;

    private const int k_numberOfInputs = 4;
    private const int k_numberOfOutputs = 1;
    private const int numberOfChildren = 5;

    private GameObject parentGameObject;
    private EvolutionHelper evolutionHelper;
    private NeatGenomeDecoder genomeDecoder;
    private ArtefactEvaluator.EvaluationInfo evaluationInfo;
    private List<NeatGenome> seeds = new List<NeatGenome>(); 
    private List<GameObject> seedsGameObjects = new List<GameObject>(); 

    void Start () 
	{ 
        evolutionHelper = new EvolutionHelper(k_numberOfInputs, k_numberOfOutputs);
        var intialGenome = evolutionHelper.CreateInitialGenome();
        genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());

        parentGameObject = CreateGameObject("Parent");
        //Camera.main.GetComponent<CameraMouseOrbit>().target = parentGameObject.transform;

        for (int i = 0; i < numberOfChildren; i++)
        {
            var child = CreateGameObject("Child" + (i + 1));
            var direction = Quaternion.Euler(0f, 0f, -(180f/(numberOfChildren - 1))*i) * Vector3.left;
            child.transform.position = direction * 30f;
            seedsGameObjects.Add(child);
        }

        SpawnParentAndSeeds(intialGenome);
	}

    void OnValidate()
    {
        ArtefactEvaluator.DefaultInputType = InputType;
    }

    void Update () 
	{
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                var index = seedsGameObjects.IndexOf(hitInfo.collider.gameObject);
                if (index >= 0)
                {
                    SpawnParentAndSeeds(seeds[index]);
                    Debug.Log("Current generation: " + seeds[index].BirthGeneration);
                }
            }
        }
	}

    void CreateMesh(NeatGenome genome, GameObject attachedGameObject)
    {
        var phenome = genomeDecoder.Decode(genome);

        Mesh mesh = ArtefactEvaluator.Evaluate(phenome, m_voxelVolume, out evaluationInfo);

        mesh.RecalculateNormals();
        //The diffuse shader wants uvs so just fill with a empty array, they're not actually used
        mesh.uv = new Vector2[mesh.vertices.Length];
        // destroy mesh object to free up memory
        GameObject.DestroyImmediate(attachedGameObject.GetComponent<MeshFilter>().mesh);

        attachedGameObject.GetComponent<MeshFilter>().mesh = mesh;

        var meshCollider = attachedGameObject.GetComponent<MeshCollider>();
        if (meshCollider != null)
            Destroy(meshCollider);
        attachedGameObject.AddComponent<MeshCollider>();
    }

    void SpawnParentAndSeeds(NeatGenome parent)
    {
        CreateMesh(parent, parentGameObject);

        seeds.Clear();
        for (int i = 0; i < numberOfChildren; i++)
        {
            var child = evolutionHelper.MutateGenome(parent);
            seeds.Add(child);
            CreateMesh(child, seedsGameObjects[i]);
        }
    }

    GameObject CreateGameObject(string name)
    {
        var meshGameObject = new GameObject(name);
        meshGameObject.AddComponent<MeshFilter>();
        meshGameObject.AddComponent<MeshRenderer>();
        meshGameObject.AddComponent<ProceduralMesh>();
        meshGameObject.AddComponent<RotateAround>().speed = 15;
        meshGameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
        return meshGameObject;
    }
}
