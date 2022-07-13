using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : MonoBehaviour
{
    AudioSource[] audiosources;

    // Start is called before the first frame update
    void Start()
    {
        audiosources = GetComponentsInChildren<AudioSource>();
        int random = UnityEngine.Random.Range(0, BackGroundMusicManager.instance.clips.Length);
        
        for(int x = 0; x < audiosources.Length; x++)
        {
            audiosources[x].clip = BackGroundMusicManager.instance.clips[random];
            audiosources[x].loop = true;
            audiosources[x].volume = 1;
            audiosources[x].maxDistance = 650;
            audiosources[x].Play();
        }
    }
}
