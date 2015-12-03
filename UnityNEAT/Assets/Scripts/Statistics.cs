using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Statistics
{
    public int totalOfPlantedArtefacts;
    public uint maxGeneration;

    // players identified based on username
    public Dictionary<string, UserStatistics> players;
    // planted artefacts based on genomeID
    public Dictionary<uint, ArtefactStatistics> artefacts;

    // This collection represents the distribution of artefacts over color
    public Dictionary<Color, int> colorDistribution;
    //This collection represents the distribution of artefacts over generations
    public Dictionary<uint, int> numberOfObjectsPerGeneration;

    private static Statistics _instance;
    public static Statistics Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            else
            {
                return _instance = new Statistics();
            }
        }
    }

    public Statistics()
    {
        players = new Dictionary<string, UserStatistics>();
        artefacts = new Dictionary<uint, ArtefactStatistics>();
    }

    public void AddPlayer(string name)
    {
        if (players.ContainsKey(name)) return;

        players.Add(name, new UserStatistics(name));
    }

    public void AddArtefact(uint genomeID, uint generation)
    {
        artefacts.Add(genomeID, new ArtefactStatistics(genomeID, generation));

        totalOfPlantedArtefacts++;

        if (generation > maxGeneration)
            maxGeneration = generation;
    }


}
