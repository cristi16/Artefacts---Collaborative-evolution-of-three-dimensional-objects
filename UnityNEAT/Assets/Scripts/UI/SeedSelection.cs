using UnityEngine;
using System.Collections;

public class SeedSelection
{
    public ArtefactSeed seed;
    public GameObject selectionGfx;
    public int indexInInventory;

    public SeedSelection(ArtefactSeed seed, GameObject selectionGfx, int index)
    {
        this.seed = seed;
        this.selectionGfx = selectionGfx;
        this.indexInInventory = index;
    }
}
