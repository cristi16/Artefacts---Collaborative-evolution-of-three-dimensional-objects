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

    private uint idCount;
    private string serverStartTime;
    private string savePath;

    private Statistics statistics = new Statistics();

    public override void OnStartServer()
    {
        base.OnStartServer();

        serverStartTime = DateTime.Now.ToString("dd.MM.yy-hh.mm");
        savePath = Application.persistentDataPath + "/" + serverStartTime;
        Directory.CreateDirectory(savePath);

        evolutionHelper = EvolutionHelper.Instance;

        // Spawn Initial Artefact
        var initialGenome = evolutionHelper.CreateInitialGenome();
        //for (int i = 0; i < 10; i++)
        //    initialGenome = evolutionHelper.MutateGenome(initialGenome);

        StartCoroutine(SpawnArtefactWithSeeds(initialGenome, Vector3.up * 0.5f, Vector3.zero));
    }

    public void SpawnSeedFromMutation(uint seedID, Vector3 spawnPosition, Vector3 eulerAngles)
    {
        StartCoroutine(SpawnArtefactWithSeeds(seedsDictionary[seedID], spawnPosition, eulerAngles));

        //SaveGenome(seedsDictionary[seedID], seedID + ".gnm.xml");
    }

    public void SpawnCrossoverResult(string serializedGenome, Vector3 spawnPosition, Vector3 eulerAngles)
    {
        var genome = NeatGenomeXmlIO.ReadGenome(XmlReader.Create(new StringReader(serializedGenome)), true);
        genome.GenomeFactory = evolutionHelper.GenomeFactory;

        //Genome ID is wrong because by doing the crossover on the client the genome factory is not the same as the one on the server
        // Therefore, we create a copy and generate a valid ID. This way all spawned genomes have unique IDs
        var validGenome = evolutionHelper.GenomeFactory.CreateGenomeCopy(genome,
            evolutionHelper.GenomeFactory.NextGenomeId(), genome.BirthGeneration);

        StartCoroutine(SpawnArtefactWithSeeds(validGenome, spawnPosition, eulerAngles));

        //SaveGenome(validGenome, validGenome.Id + ".gnm.xml");
    }

    public void DeleteSeed(uint seedID)
    {
        seedsDictionary.Remove(seedID);
    }

    IEnumerator SpawnArtefactWithSeeds(NeatGenome genome, Vector3 spawnPosition, Vector3 eulerAngles)
    {
        // Spawn Parent
        var artefactInstance = CreateArtefactInstance<Artefact>(genome, artefactPrefab, spawnPosition, eulerAngles);
        NetworkServer.Spawn(artefactInstance.gameObject);

        yield return new WaitForSeconds(Artefact.k_growthTime);

        // Spawn Seeds
        for(int i = 0; i < k_numberOfSeeds; i++)
        {
            var seedGenome = evolutionHelper.MutateGenome(genome);
            var direction = Quaternion.Euler(0f, (360f / k_numberOfSeeds) * i, 0f) * Vector3.forward;

            var seedInstance = CreateArtefactInstance<ArtefactSeed>(seedGenome, seedPrefab, artefactInstance.transform.position + direction * 2f, Quaternion.LookRotation(direction).eulerAngles);
            seedInstance.ID = seedGenome.Id;
            seedInstance.facingDirection = direction;

            NetworkServer.Spawn(seedInstance.gameObject);

            seedsDictionary.Add(seedInstance.ID, seedGenome);      
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
}
