using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Sound
{
    public class BGMFade : MonoBehaviour
    {
        public AudioSource preBGM, nextBGM, introBGM;
        public float fadeTime;
        public float optionVolume = 1f;
        private float fadeDeltaTime;
        private bool isFadeOver;
        private bool bgmSwitch;
        // Start is called before the first frame update
        void Start()
        {
            isFadeOver = false;
            bgmSwitch = isFadeOver;
        }

        // Update is called once per frame
        void Update()
        {
            fadeDeltaTime += Time.deltaTime;
            FadeOut();
        }

        private void FadeOut()
        {
            preBGM.volume = (1f - fadeDeltaTime / fadeTime) * optionVolume;
            if (fadeDeltaTime >= fadeTime)
            {
                isFadeOver = true;
            }
            if (isFadeOver == true && bgmSwitch == false)
            {
                bgmSwitch = true;
                preBGM.Stop();
                if (introBGM != null)
                {
                    SoundManager.IntroLoopPlay(introBGM, nextBGM, optionVolume);
                }
                else
                {
                    nextBGM.volume = optionVolume;
                    nextBGM.Play();
                }
                Destroy(gameObject);

            }
        }
    }
}


