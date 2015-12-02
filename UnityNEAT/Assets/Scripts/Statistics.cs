using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Statistics
{
    public int totalOfPlantedArtefacts;
    public int maxGeneration;

    // players identified based on username
    public Dictionary<string, UserStatistics> players;
    // planted artefacts based on genomeID
    public Dictionary<uint, ArtefactStatistics> artefacts;

    // This collection keeps track of the number of planted objects associated with one color
    public Dictionary<Color, int> colorDistribution;

    public Dictionary<int, int> numberOfObjectsPerGeneration;

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
    }
}
