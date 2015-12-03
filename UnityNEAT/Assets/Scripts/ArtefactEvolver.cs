using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using SharpNeat.Genomes.Neat;
using UnityEngine;
using UnityEngine.Networking;

// This class is Server-Side Only
public class ArtefactEvolver : NetworkBehaviour
{
    public GameObject artefactPrefab;
    public GameObject seedPrefab;

    private EvolutionHelper evolutionHelper;
    // maps seed unique ID to seed genome
    private Dictionary<uint, NeatGenome> seedsDictionary = new Dictionary<uint, NeatGenome>();
    private const int k_numberOfSeeds = 5;
    private const int k_numberOfPreEvolutions = 5;
    private const int k_numberOfInitialSeeds = 10;

    private uint idCount;
    private string serverStartTime;
    private string savePath;

    public override void OnStartServer()
    {
        base.OnStartServer();

        serverStartTime = DateTime.Now.ToString("dd.MM.yy-hh.mm");
        savePath = Application.persistentDataPath + "/" + serverStartTime;
        Directory.CreateDirectory(savePath);

        evolutionHelper = EvolutionHelper.Instance;

        // Spawn Initial Artefact
        var initialGenome = evolutionHelper.CreateInitialGenome();
        // Perform a number of mutations on the initial genome so that it doesn't result in a boring looking cube
        for (int i = 0; i < k_numberOfPreEvolutions; i++)
            initialGenome = evolutionHelper.MutateGenome(initialGenome);

        StartCoroutine(SpawnInitialArtefacts(initialGenome));
        //StartCoroutine(SpawnArtefactWithSeeds(initialGenome, Vector3.up * 5, Quaternion.identity.eulerAngles, initialGenome.Id));
        StartCoroutine(SaveStatistics());
    }

    public void SpawnSeedFromMutation(uint seedID, Vector3 spawnPosition, Vector3 eulerAngles, string playerName, uint parent)
    {
        StartCoroutine(SpawnArtefactWithSeeds(seedsDictionary[seedID], spawnPosition, eulerAngles, parent));

        SaveGenome(seedsDictionary[seedID], seedID + ".gnm.xml");

        Statistics.Instance.totalOfPlantedArtefacts++;
        Statistics.Instance.AddArtefact(seedID, seedsDictionary[seedID].BirthGeneration);
        var playerStatistics = Statistics.Instance.players[playerName];
        playerStatistics.numberOfPlanedArtefact++;
        playerStatistics.numberOfMutations++;
        playerStatistics.plantedObjects.Add(seedID);
        Statistics.Instance.artefacts[seedID].usersInteracted.Add(playerName);
    }

    public void SpawnCrossoverResult(string serializedGenome, Vector3 spawnPosition, Vector3 eulerAngles, string playerName, uint parent1, uint parent2)
    {
        var genome = NeatGenomeXmlIO.ReadGenome(XmlReader.Create(new StringReader(serializedGenome)), true);
        genome.GenomeFactory = evolutionHelper.GenomeFactory;

        //Genome ID is wrong because by doing the crossover on the client the genome factory is not the same as the one on the server
        // Therefore, we create a copy and generate a valid ID. This way all spawned genomes have unique IDs
        var validGenome = evolutionHelper.GenomeFactory.CreateGenomeCopy(genome,
            evolutionHelper.GenomeFactory.NextGenomeId(), genome.BirthGeneration);

        StartCoroutine(SpawnArtefactWithSeeds(validGenome, spawnPosition, eulerAngles, parent1, parent2));

        SaveGenome(validGenome, validGenome.Id + ".gnm.xml");

        Statistics.Instance.totalOfPlantedArtefacts++;
        Statistics.Instance.AddArtefact(validGenome.Id, validGenome.BirthGeneration);
        var playerStatistics = Statistics.Instance.players[playerName];
        playerStatistics.numberOfPlanedArtefact++;
        playerStatistics.numberOfCrossovers++;
        playerStatistics.plantedObjects.Add(validGenome.Id);
        Statistics.Instance.artefacts[validGenome.Id].usersInteracted.Add(playerName);
    }

    public void DeleteSeed(uint seedID)
    {
        seedsDictionary.Remove(seedID);
    }

    IEnumerator SpawnArtefactWithSeeds(NeatGenome genome, Vector3 spawnPosition, Vector3 eulerAngles, uint parent1 = 0, uint parent2 = 0)
    {
        // this is to ensure that the player is the first thing getting spawned
        yield return new WaitForEndOfFrame();

        // Spawn Parent
        var artefactInstance = CreateArtefactInstance<Artefact>(genome, artefactPrefab, spawnPosition, eulerAngles);

        artefactInstance.Parent1Id = parent1;
        artefactInstance.Parent1Id = parent2;
        if (Statistics.Instance.artefacts.ContainsKey(genome.Id))
        {
            Statistics.Instance.artefacts[genome.Id].AddParents(parent1, parent2);
            Statistics.Instance.artefacts[genome.Id].AddUsersFromParents(parent1, parent2);
        }
        if (Statistics.Instance.artefacts.ContainsKey(parent1))
            Statistics.Instance.artefacts[parent1].numberOfSeedsReplanted++;
        if (Statistics.Instance.artefacts.ContainsKey(parent2))
            Statistics.Instance.artefacts[parent2].numberOfSeedsReplanted++;

        NetworkServer.Spawn(artefactInstance.gameObject);

        yield return new WaitForSeconds(Artefact.k_growthTime);

        // Spawn Seeds
        for(int i = 0; i < k_numberOfSeeds; i++)
        {
            var seedGenome = evolutionHelper.MutateGenome(genome);
            var direction = Quaternion.Euler(0f, (360f / k_numberOfSeeds) * i, 0f) * Vector3.forward;

            var seedInstance = CreateArtefactInstance<ArtefactSeed>(seedGenome, seedPrefab, artefactInstance.transform.position + direction * 3f, Quaternion.LookRotation(direction).eulerAngles);
            seedInstance.facingDirection = direction;

            seedInstance.Parent1Id = genome.Id;
            seedInstance.Parent2Id = genome.Id;

            NetworkServer.Spawn(seedInstance.gameObject);

            seedsDictionary.Add(seedInstance.GenomeId, seedGenome);      
        }
    }

    private T CreateArtefactInstance<T>(NeatGenome genome, GameObject prefab ,Vector3 initialPosition, Vector3 initialRotation) where T: Artefact
    {
        var artefactInstance = Instantiate(prefab);

        var serializedGenome = NeatGenomeXmlIO.Save(genome, true).OuterXml;
        //var byteCount = System.Text.ASCIIEncoding.ASCII.GetByteCount(serializedGenome);
        //Debug.LogWarning("Byte count: " + byteCount); 

        var artefact = artefactInstance.GetComponent<T>();
        artefact.SerializedGenome = serializedGenome;
        artefact.GenomeId = genome.Id;
        artefact.transform.position = initialPosition;
        artefact.transform.eulerAngles = initialRotation;
        return artefact;
    }

    private void SaveGenome(NeatGenome genome , string fileName)
    {
        XmlWriterSettings _xwSettings = new XmlWriterSettings();
        _xwSettings.Indent = true;
        using (XmlWriter xw = XmlWriter.Create(savePath + "/" + fileName, _xwSettings))
        {
            NeatGenomeXmlIO.WriteComplete(xw, genome, true);
        }
    }

    IEnumerator SaveStatistics()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);
            //Statistics.Instance.Serialize(serverStartTime);
        }
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        Statistics.Instance.Serialize(savePath);
        Debug.Log("Server destroyed");
    }

    IEnumerator SpawnInitialArtefacts(NeatGenome initialGenome)
    {
        for (int i = 0; i < k_numberOfInitialSeeds; i++)
        {
            yield return new WaitForEndOfFrame();
            var mutatedGenome = evolutionHelper.MutateGenome(initialGenome);

            var mutationCount = UnityEngine.Random.Range(1, 5);
            for (int j = 0; j < mutationCount; j++)
            {
                mutatedGenome = evolutionHelper.MutateGenome(initialGenome);
            }

            var direction = Quaternion.Euler(0f, (360f / k_numberOfInitialSeeds) * i, 0f) * Vector3.forward;         
            var position = direction * UnityEngine.Random.Range(15f, 50f);
            position.y = 2f;
            StartCoroutine(SpawnArtefactWithSeeds(mutatedGenome, position, Quaternion.LookRotation(direction).eulerAngles, initialGenome.Id));
        }
    }
}
