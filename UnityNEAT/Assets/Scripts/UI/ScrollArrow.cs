using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollArrow : MonoBehaviour
{
    public float direction;
    private Animation animation;

    void Start()
    {
        animation = GetComponent<Animation>();
    }

    void Update()
    {
        var scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) >= 0.1f)
        {
            if(scrollInput * direction > 0f)
                animation.Play();
        }
    }
}
