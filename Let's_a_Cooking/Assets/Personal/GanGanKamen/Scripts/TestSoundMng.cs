using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSoundMng : MonoBehaviour
{
    [SerializeField] AudioSource preBGM = null;
    [SerializeField] AudioSource newBGM = null;
    [SerializeField] float fadeTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchBGM()
    {
        Cooking.SoundManager.SwitchBGM(preBGM, newBGM, fadeTime);
    }
}
