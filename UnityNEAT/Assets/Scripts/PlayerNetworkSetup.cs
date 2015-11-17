using System.Collections.Generic;
using HighlightingSystem;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerNetworkSetup : NetworkBehaviour
{
    private Ray ray;
    private RaycastHit hitInfo;
    private const float rayDistance = 3f;

    private ArtefactEvolver evolver;

    private ScrollViewLayout scrollView;

    private List<ArtefactSeed> collectedSeeds;

    public override void OnStartServer()
    {
        base.OnStartServer();
        evolver = FindObjectOfType<ArtefactEvolver>();
    }

    public void Start()
    {
        if (isLocalPlayer == false)
        {
            GetComponent<CharacterController>().enabled = false;
            GetComponent<FirstPersonController>().enabled = false;
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
        }
        else
        {
            scrollView = FindObjectOfType<ScrollViewLayout>();
            collectedSeeds = new List<ArtefactSeed>();

            Cursor.visible = false;
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
                    scrollView.Reset();

                    collectedSeeds.Add(hitInfo.collider.GetComponent<ArtefactSeed>());
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                CmdSpawnSeed(collectedSeeds[scrollView.selectedIndex].ID, transform.position + transform.forward * 5f + transform.up * 5f);
            }
        }
    }

    [Command]
    void CmdSpawnSeed(uint seedID, Vector3 spawnPosition)
    {
        evolver.SpawnSeed(seedID, spawnPosition);
    }
}
