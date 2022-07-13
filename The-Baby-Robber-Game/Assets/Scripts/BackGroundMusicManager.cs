using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundMusicManager : MonoBehaviour
{
    public static BackGroundMusicManager instance;
    public AudioClip[] clips;

    private void Awake()
    {
        instance = this;
    }

}
