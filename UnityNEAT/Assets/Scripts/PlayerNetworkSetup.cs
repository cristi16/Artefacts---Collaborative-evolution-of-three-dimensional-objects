using System.Collections.Generic;
using System.IO;
using HighlightingSystem;
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

    private List<SeedSelection> seedSelections = new List<SeedSelection>(); 

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
        if (isLocalPlayer)
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F))
                GetComponent<FirstPersonController>().IsFrozen = !GetComponent<FirstPersonController>().IsFrozen;

            ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * rayDistance);

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

            if (scrollView.transform.childCount == 0) return;

            var currentlySelectedSeed = collectedSeeds[scrollView.selectedIndex];

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (HasSelectedSeed(currentlySelectedSeed))
                {
                    DeselectSeed(currentlySelectedSeed);
                }
                else if (seedSelections.Count < 3)
                {
                    SelectSeed(currentlySelectedSeed);
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                CmdSpawnSeed(collectedSeeds[scrollView.selectedIndex].ID, transform.position + transform.forward * 5f + transform.up * 5f);
                if (evolver.destroySeedsOnPlacement)
                {
                    CmdDestroySeed(collectedSeeds[scrollView.selectedIndex].ID);
                    CmdDestroySeedObject(collectedSeeds[scrollView.selectedIndex].netId);
                    collectedSeeds.RemoveAt(scrollView.selectedIndex);
                    if (scrollView.selectedIndex > 0)
                        scrollView.selectedIndex--;
                }
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
        selectionGfx.transform.localScale = seedSelectionGfx.transform.localScale;
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
