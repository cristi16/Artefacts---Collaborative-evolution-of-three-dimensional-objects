using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ShowSideUI : MonoBehaviour
{
    private Animation animation;
    private List<AnimationState> states;
    void Start()
    {
        animation = GetComponent<Animation>();
        states = new List<AnimationState>(animation.Cast<AnimationState>());

        if (PlayerPrefs.GetInt("firstSeed") == 1)
            animation.Play(states[0].name);
        if (PlayerPrefs.GetInt("secondSeed") == 1)
            animation.Play(states[1].name);

    }

    public void ShowUI(int numberOfCollectedSeeds)
    {
        if (numberOfCollectedSeeds == 1 && PlayerPrefs.GetInt("firstSeed") == 0)
        {
            PlayerPrefs.SetInt("firstSeed", 1);
            animation.Play(states[0].name);
        }
        else if (numberOfCollectedSeeds == 5 && PlayerPrefs.GetInt("secondSeed") == 0)
        {
            PlayerPrefs.SetInt("secondSeed", 1);
            animation.Play(states[1].name);
        }
    }
}
