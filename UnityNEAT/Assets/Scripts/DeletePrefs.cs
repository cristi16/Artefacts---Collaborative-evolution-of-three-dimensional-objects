using UnityEngine;
using System.Collections;

public class DeletePrefs : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    public void Delete()
    {
        PlayerPrefs.DeleteAll();
    }
}
