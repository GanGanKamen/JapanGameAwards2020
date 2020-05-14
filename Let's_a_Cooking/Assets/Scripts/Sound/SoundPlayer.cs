using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// SEやBGMの再生を制御
    /// </summary>
    public class SoundPlayer : MonoBehaviour
    {
        public static void IntroLoopPlay(AudioSource introSource, AudioSource loopSource) //イントロ付きループ再生
        {
            //イントロ部分のオーディオソードとループ部分のオーディオソースを用意する
            introSource.loop = false;
            introSource.Play();
            loopSource.loop = true;
            loopSource.PlayScheduled(AudioSettings.dspTime - 0.1f + introSource.clip.length);
        }

        public static void IntroLoopPlay(AudioSource introSource, AudioSource loopSource, float OptionVolume) //ゲームオプションの音量調節
        {
            introSource.loop = false;
            introSource.Play();
            loopSource.loop = true;
            loopSource.PlayScheduled(AudioSettings.dspTime - 0.1f + introSource.clip.length);
        }

        public static void SwitchBGM(AudioSource preBGM, AudioSource newBgm, float time) //現在再生中のBGMをフェードアウトで新しいBGMに切り替える
        {
            GameObject fadeObj = new GameObject();
            fadeObj.AddComponent<Cooking.Sound.BGMFade>();
            Cooking.Sound.BGMFade fade = fadeObj.GetComponent<Cooking.Sound.BGMFade>();
            fade.preBGM = preBGM;
            fade.nextBGM = newBgm;
            fade.fadeTime = time;
        }

        public static void SwitchBGM(AudioSource preBGM, AudioSource newBgm, float time, float OptionVolume) //ゲームオプションの音量調節
        {
            GameObject fadeObj = new GameObject();
            fadeObj.AddComponent<Cooking.Sound.BGMFade>();
            Cooking.Sound.BGMFade fade = fadeObj.GetComponent<Cooking.Sound.BGMFade>();
            fade.preBGM = preBGM;
            fade.nextBGM = newBgm;
            fade.fadeTime = time;
            fade.optionVolume = OptionVolume;
        }

        public static void SwitchBGM(AudioSource preBGM, AudioSource newBgmIntro, AudioSource newBGMLoop, float time) //フェードアウトからのイントロ付きループ再生
        {
            GameObject fadeObj = new GameObject();
            fadeObj.AddComponent<Cooking.Sound.BGMFade>();
            Cooking.Sound.BGMFade fade = fadeObj.GetComponent<Cooking.Sound.BGMFade>();
            fade.preBGM = preBGM;
            fade.introBGM = newBgmIntro;
            fade.nextBGM = newBGMLoop;
            fade.fadeTime = time;
        }

        public static void SwitchBGM(AudioSource preBGM, AudioSource newBgmIntro, AudioSource newBGMLoop, float time, float OptionVolume)
        {
            GameObject fadeObj = new GameObject();
            fadeObj.AddComponent<Cooking.Sound.BGMFade>();
            Cooking.Sound.BGMFade fade = fadeObj.GetComponent<Cooking.Sound.BGMFade>();
            fade.preBGM = preBGM;
            fade.introBGM = newBgmIntro;
            fade.nextBGM = newBGMLoop;
            fade.fadeTime = time;
            fade.optionVolume = OptionVolume;
        }

        public static void PlayBGM(AudioSource audioSource)
        {
            audioSource.Play();
        }

        public static void PlayBGM(AudioSource audioSource, float OptionVolume) //ゲームオプションの音量調節に適応するBGM再生
        {
            audioSource.volume = OptionVolume;
            audioSource.Play();
        }

        public static void PlaySEOneTime(AudioSource audioSource, AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }

        public static void PlaySEOneTime(AudioSource audioSource, AudioClip clip, float OptionVolume) //ゲームオプションの音量調節に適応するSE再生
        {
            audioSource.volume = OptionVolume;
            audioSource.PlayOneShot(clip);
        }
        /// <summary>
        /// ループにも対応したSE再生
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="soundInformation"></param>
        public static void PlaySE(AudioSource audioSource,  SoundParameter.SoundInformation soundInformation) //ゲームオプションの音量調節に適応するSE再生
        {
            audioSource.volume = soundInformation.volume;
            if (soundInformation.loop)
            {
                audioSource.loop = true;
            }
            audioSource.Play();
        }

        /// <summary>
        /// 3Dサウンドの再生
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="position"></param>
        public static void Play3DSEOneTime(AudioClip clip , Vector3 position)
        {
            AudioSource.PlayClipAtPoint(clip, position);
        }
    }
}

