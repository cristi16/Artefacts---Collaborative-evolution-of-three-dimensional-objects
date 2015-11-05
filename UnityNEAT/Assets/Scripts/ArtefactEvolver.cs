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

    public override void OnStartServer()
    {
        base.OnStartServer();

        evolutionHelper = new EvolutionHelper(4, 1);

        // Spawn Initial Artefact
        var initialGenome = evolutionHelper.CreateInitialGenome();

        var artefactInstance = Instantiate(artefactPrefab);

        var serializedGenome = NeatGenomeXmlIO.Save(initialGenome, true).OuterXml;
        artefactInstance.GetComponent<Artefact>().SerializedGenome = serializedGenome;

        NetworkServer.Spawn(artefactInstance);
    }
}
