using UnityEngine;
using System.Collections.Generic;

public class UserStatistics
{
    public string name;

    public int numberOfPlanedArtefact;
    public int numberOfMutations;
    public int numberOfCrossovers;

    public int numberOfSeedsPickedUp;
    // list of genomeIDs planted
    public List<uint> plantedObjects;

    public UserStatistics(string name)
    {
        this.name = name;
        plantedObjects = new List<uint>();
    }
}