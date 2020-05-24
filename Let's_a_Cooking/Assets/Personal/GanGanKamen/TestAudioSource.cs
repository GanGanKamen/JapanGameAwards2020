using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.Audio;

public class TestAudioSource : MonoBehaviour
{
    [SerializeField] AudioMixer mixer;
    [SerializeField] AudioMixerGroup mixerGroup;
    // Start is called before the first frame update
    void Start()
    {
        //var audio = GetComponent<AudioSource>();
        //var mixerGroup0 = mixer.FindMatchingGroups("Master/BGM");
        //audio.outputAudioMixerGroup = mixerGroup0[0];
        var soundManager = GameObject.Find("SoundManager").GetComponent<Cooking.SoundManager>();
        soundManager.PlaySE(Cooking.SoundEffectID.battle_start0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
