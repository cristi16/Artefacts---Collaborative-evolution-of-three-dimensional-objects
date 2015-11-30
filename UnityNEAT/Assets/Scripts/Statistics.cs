using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Statistics
{
    public GeneralStatistics general;
    public UserStatistics user;
    public ArtefactStatistics artefact;

    public Statistics()
    {
        general = new GeneralStatistics();
        user = new UserStatistics();
        artefact = new ArtefactStatistics();
    }

    public class GeneralStatistics
    {
        public int totalOfPlantedArtefacts;

        public List<string> players;
        // most evolved object
        public int maxGeneration;
        // This collection keeps track of the number of planted objects associated with one color
        public Dictionary<Color, int> colorDistribution;
    }

    public class UserStatistics
    {
        
    }

    public class ArtefactStatistics
    {
        
    }
}
