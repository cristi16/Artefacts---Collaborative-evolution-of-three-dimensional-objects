using System.Collections.Generic;
using SharpNeat.Genomes.Neat;
using UnityEngine;
using UnityEngine.Networking;

// This class is Server-Only
public class ArtefactEvolver : NetworkBehaviour
{
    public GameObject artefactPrefab;

    private EvolutionHelper evolutionHelper;
    // unique identifier used to locate genome is network id of Artefact
    private Dictionary<int, NeatGenome> genomeDictionary;
    private const int k_numberOfInputs = 4;
    private const int k_numberOfOutputs = 1;

    public override void OnStartServer()
    {
        base.OnStartServer();

        evolutionHelper = new EvolutionHelper(k_numberOfInputs, k_numberOfOutputs);

        // Spawn Initial Artefact
        var initialGenome = evolutionHelper.CreateInitialGenome();

        var artefactInstance = Instantiate(artefactPrefab);

        var serializedGenome = NeatGenomeXmlIO.Save(initialGenome, true).OuterXml;
        //var byteCount = System.Text.ASCIIEncoding.ASCII.GetByteCount(doc.OuterXml);
        //Debug.LogWarning("Byte count: " + byteCount); 

        artefactInstance.GetComponent<Artefact>().SerializedGenome = serializedGenome;

        NetworkServer.Spawn(artefactInstance);
    }
}
