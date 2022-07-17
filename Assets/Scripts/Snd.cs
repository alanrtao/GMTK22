using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snd : MonoBehaviour
{
    public AudioSource src;
    public List<AudioClip> clips;

    public static Snd Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public static void Play(string name)
    {
        foreach( var s in Instance.clips )
        {
            if (s.name.Equals(name))
            {
                Instance.src.PlayOneShot(s);
                break;
            }
        }
    }
}
