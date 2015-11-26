using UnityEngine;
using System.Collections;

public class SelectionGraphics : MonoBehaviour
{

    void LateUpdate()
    {
        transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}
