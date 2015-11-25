using UnityEngine;
using System.Collections;

public class SeedSelection
{
    public ArtefactSeed seed;
    public GameObject selectionGfx;

    public SeedSelection(ArtefactSeed seed, GameObject selectionGfx)
    {
        this.seed = seed;
        this.selectionGfx = selectionGfx;
    }
}
