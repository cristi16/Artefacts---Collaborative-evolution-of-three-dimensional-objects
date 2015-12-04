using UnityEngine;
using System.Collections;

public class DebugMode : MonoBehaviour
{
    public GameObject panel;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
            panel.SetActive(!panel.activeInHierarchy);
    }
}
