using UnityEngine;
using System.Collections;

public class HelpScreen : MonoBehaviour
{

    public GameObject help;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            help.SetActive(!help.activeInHierarchy);
        }
    }
}
