using System.Collections.Generic;
using System.IO;
using System.Xml;
using HighlightingSystem;
using SharpNeat.Genomes.Neat;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerNetworkSetup : NetworkBehaviour
{
    public List<Color> selectionColors;
    public GameObject seedSelectionGfx;

    private Ray ray;
    private RaycastHit hitInfo;
    private const float rayDistance = 3f;

    private ArtefactEvolver evolver;
    private ScrollViewLayout scrollView;
    private List<ArtefactSeed> collectedSeeds;
    private string PlayerName;
    private bool isPlacingSeeds;

    private List<SeedSelection> seedSelections = new List<SeedSelection>();
    private ArtefactSeed placeholderArtefact;
    private float artefactScale = 0.1326183f;

    public override void OnStartServer()
    {
        base.OnStartServer();
        evolver = FindObjectOfType<ArtefactEvolver>();
    }

    public void Start()
    {
        if (isLocalPlayer == false)
        {
            GetComponent<FirstPersonController>().enabled = false;
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
        }
        else
        {
            scrollView = FindObjectOfType<ScrollViewLayout>();
            collectedSeeds = new List<ArtefactSeed>();

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            PlayerName = PlayerPrefs.GetString("PlayerName");
            gameObject.name = PlayerName;
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F))
            GetComponent<FirstPersonController>().IsFrozen = !GetComponent<FirstPersonController>().IsFrozen;

        if (isPlacingSeeds == false)
        {
            ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            //Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayDistance);

            if (Physics.Raycast(ray, out hitInfo, rayDistance, LayerMask.GetMask("Seed")))
            {
                hitInfo.collider.GetComponent<Highlighter>().On(Color.green);

                if (Input.GetMouseButtonDown(0))
                {
                    hitInfo.collider.transform.parent = scrollView.transform;
                    hitInfo.collider.gameObject.layer = LayerMask.NameToLayer("UI");
                    scrollView.Reset();

                    collectedSeeds.Add(hitInfo.collider.GetComponent<ArtefactSeed>());
                }
            }
        }

        if (scrollView.transform.childCount == 0) return;

        var currentlySelectedSeed = collectedSeeds[scrollView.selectedIndex];

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (HasSelectedSeed(currentlySelectedSeed))
            {
                DeselectSeed(currentlySelectedSeed);
                if (seedSelections.Count == 0)
                {
                    isPlacingSeeds = false;
                }
            }
            else if (seedSelections.Count < 3)
            {
                if (seedSelections.Count == 0)
                {
                    isPlacingSeeds = true;
                    placeholderArtefact = Instantiate(currentlySelectedSeed);
                    placeholderArtefact.transform.position = Camera.main.transform.position +
                                                             Camera.main.transform.forward * Dragable.k_DragDistance;
                    placeholderArtefact.transform.localScale = Vector3.one * artefactScale;
                    placeholderArtefact.GetComponent<Rigidbody>().mass = 0.01f;
                    placeholderArtefact.gameObject.AddComponent<Dragable>().StartDragging();
                }
                SelectSeed(currentlySelectedSeed);
            }
        }

        if (Input.GetMouseButtonDown(1) && isPlacingSeeds)
        {
            CmdSpawnSeed(currentlySelectedSeed.ID, transform.position + transform.forward * 5f + transform.up * 5f);
            if (evolver.destroySeedsOnPlacement)
            {
                CmdDestroySeed(currentlySelectedSeed.ID);
                CmdDestroySeedObject(currentlySelectedSeed.netId);
                collectedSeeds.RemoveAt(scrollView.selectedIndex);
                if (scrollView.selectedIndex > 0)
                    scrollView.selectedIndex--;
            }
        }
    }

    [Command]
    void CmdSpawnSeed(uint seedID, Vector3 spawnPosition)
    {
        evolver.SpawnSeed(seedID, spawnPosition);
    }

    [Command]
    void CmdDestroySeed(uint seedID)
    {
        //evolver
        evolver.DeleteSeed(seedID);
    }

    [Command]
    void CmdDestroySeedObject(NetworkInstanceId netID)
    {
        var seedObject = NetworkServer.FindLocalObject(netID);
        NetworkServer.Destroy(seedObject);
    }

    [Command]
    void CmdSpawnFromCrossoverResult(string serializedCrossoverResult)
    {
        
    }

    public void CombineSeeds(ArtefactSeed seed1, ArtefactSeed seed2)
    {
        var genome1 =  NeatGenomeXmlIO.ReadGenome(XmlReader.Create(new StringReader(seed1.SerializedGenome)), true);
        var genome2 =  NeatGenomeXmlIO.ReadGenome(XmlReader.Create(new StringReader(seed2.SerializedGenome)), true);

        var result = genome1.CreateOffspring(genome2, (uint)Mathf.Max(genome1.BirthGeneration, genome2.BirthGeneration) + 1);

        var serializedGenome = NeatGenomeXmlIO.Save(result, true).OuterXml;
    }

    private bool HasSelectedSeed(ArtefactSeed seed)
    {
        foreach (var seedSelection in seedSelections)
        {
            if (seedSelection.seed == seed) return true;
        }
        return false;
    }

    public void SelectSeed(ArtefactSeed seed)
    {
        var selectionGfx = Instantiate(seedSelectionGfx);
        selectionGfx.transform.parent = seed.transform;
        selectionGfx.transform.localPosition = seedSelectionGfx.transform.position;
        var canvasScale = seed.transform.root.GetComponent<RectTransform>().localScale;
        var gfxScale = seedSelectionGfx.transform.localScale;
        gfxScale.Scale(canvasScale);
        selectionGfx.transform.localScale = gfxScale;
        selectionGfx.GetComponent<SpriteRenderer>().color = selectionColors[seedSelections.Count];

        seedSelections.Add(new SeedSelection(seed, selectionGfx));
    }

    private void DeselectSeed(ArtefactSeed seed)
    {
        var index = -1;
        for(int i = 0; i < seedSelections.Count; i++)
        {
            if (seedSelections[i].seed == seed)
            {
                Destroy(seedSelections[i].selectionGfx);
                index = i;
            }
        }
        seedSelections.RemoveAt(index);
    }
}
