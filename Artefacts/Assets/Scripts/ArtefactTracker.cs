using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SharpNeat.Genomes.Neat;

public class ArtefactTracker
{
    private static ArtefactTracker _instance;
    public static ArtefactTracker Instance
    {
        get
        {
            if (_instance != null)
                return _instance;
            else
            {
                return _instance = new ArtefactTracker();
            }
        }
    }

    public Dictionary<uint, NeatGenome> roots;
    public Dictionary<uint, Artefact> artefacts;

    public ArtefactTracker()
    {
        roots = new Dictionary<uint, NeatGenome>();
        artefacts = new Dictionary<uint, Artefact>();
    }


}
