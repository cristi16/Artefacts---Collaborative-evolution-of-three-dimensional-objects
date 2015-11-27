using UnityEngine;
using System.Collections;

public class SelectionGraphics : MonoBehaviour
{

    void LateUpdate()
    {
        transform.localPosition = Vector3.zero;
        transform.position += Vector3.forward * 4;
        transform.rotation = Quaternion.identity;
    }
}
