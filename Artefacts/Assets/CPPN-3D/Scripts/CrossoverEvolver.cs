using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using SharpNeat.Decoders;
using SharpNeat.Decoders.Neat;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;

public class CrossoverEvolver : MonoBehaviour
{
    public ArtefactEvaluator.VoxelVolume m_voxelVolume;
    public ArtefactEvaluator.InputType InputType;
    public List<uint> generations = new List<uint>();

    private const int k_numberOfInputs = 4;
    private const int k_numberOfOutputs = 4;
    private const int numberOfChildren = 5;

    private GameObject parentGameObject;
    private EvolutionHelper evolutionHelper;
    private NeatGenomeDecoder genomeDecoder;
    private ArtefactEvaluator.EvaluationInfo evaluationInfo;
    private List<NeatGenome> seeds = new List<NeatGenome>(); 
    private List<GameObject> seedsGameObjects = new List<GameObject>();

    private List<int> selectedSeeds = new List<int>();

    void Start () 
	{ 
        evolutionHelper = new EvolutionHelper(k_numberOfInputs, k_numberOfOutputs);
        var intialGenome = evolutionHelper.CreateInitialGenome();
        genomeDecoder = new NeatGenomeDecoder(NetworkActivationScheme.CreateAcyclicScheme());

        //parentGameObject = CreateGameObject("Parent");

        for (int i = 0; i < numberOfChildren; i++)
        {
            var child = CreateGameObject("Child" + (i + 1));
            var direction = Quaternion.Euler(0f, 0f, -(180f/(numberOfChildren - 1))*i) * Vector3.left;
            child.transform.position = direction * 30f;
            seedsGameObjects.Add(child);
        }

        SpawnSeeds(intialGenome);
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
                    selectedSeeds.Add(index);
                    seedsGameObjects[index].GetComponent<Renderer>().material.color = Color.magenta;
                    Debug.Log("Selected seed: " + (index + 1));
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (selectedSeeds.Count == 0 || selectedSeeds.Count > 2)
            {
                foreach (var seedIndex in selectedSeeds)
                    seedsGameObjects[seedIndex].GetComponent<Renderer>().material.color = Color.white;
                selectedSeeds.Clear();
                return;
            }

            if (selectedSeeds.Count == 1)
            {
                var index = selectedSeeds[0];
                seeds[index] = seeds[index].CreateOffspring(seeds[index].BirthGeneration + 1);

                CreateMesh(seeds[index], seedsGameObjects[index]);
                Debug.Log("Mutated seed: " + (index + 1));
            }

            if (selectedSeeds.Count == 2)
            {
                var index1 = selectedSeeds[0];
                var index2 = selectedSeeds[1];
                var seed1 = seeds[index1];
                var seed2 = seeds[index2];

                seeds[index1] = seed1.CreateOffspring(seed2,
                    (uint)Mathf.Max(seed1.BirthGeneration, seed2.BirthGeneration) + 1);
                seeds[index2] = seed2.CreateOffspring(seed1,
                    (uint)Mathf.Max(seed1.BirthGeneration, seed2.BirthGeneration) + 1);

                CreateMesh(seeds[index1], seedsGameObjects[index1]);
                CreateMesh(seeds[index2], seedsGameObjects[index2]);

                Debug.Log("Crossover between seeds: " + (index1 + 1) + " and " + (index2 + 1));
            }

            foreach (var seedIndex in selectedSeeds)
                seedsGameObjects[seedIndex].GetComponent<Renderer>().material.color = Color.white;
            selectedSeeds.Clear();
            generations.Clear();
            for(int i = 0; i < seeds.Count; i++)
                generations.Add(seeds[i].BirthGeneration);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            generations.Clear();

            for (int i = 0; i < seeds.Count; i++)
            {
                seeds[i] = seeds[i].CreateOffspring(seeds[i].BirthGeneration + 1);
                CreateMesh(seeds[i], seedsGameObjects[i]);
                generations.Add(seeds[i].BirthGeneration);
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

    void SpawnSeeds(NeatGenome parent)
    {
        //CreateMesh(parent, parentGameObject);

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
