using UnityEngine;
using System.Collections;

public class PopupUIElement : MonoBehaviour
{
    private Animation popAnimation;
    private AnimationState state;

    void Start()
    {
        popAnimation = GetComponent<Animation>();
        state = popAnimation[popAnimation.clip.name];
    }
    [ContextMenu("Popup")]
    public void PopUp()
    {
        state.speed = 1;

        if (popAnimation.isPlaying)
        {
            popAnimation.Play();
        }
        else
        {
            state.time = 0f;
            popAnimation.Play();
        }
    }
    [ContextMenu("Popdown")]
    public void PopDown()
    {
        state.speed = -1;

        if (popAnimation.isPlaying)
        {
            popAnimation.Play();
        }
        else
        {
            state.time = state.length;
            popAnimation.Play();
        }
    }
}
