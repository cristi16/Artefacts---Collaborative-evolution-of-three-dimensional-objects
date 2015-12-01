using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Statistics
{
    public int totalOfPlantedArtefacts;
    // names of players
    public List<string> players;
    // most evolved object
    public int maxGeneration;
    // This collection keeps track of the number of planted objects associated with one color
    public Dictionary<Color, int> colorDistribution;


        
}
