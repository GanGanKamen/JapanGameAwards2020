using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    public class SoundManager : MonoBehaviour
    {
        private SoundParameter _soundParameter;
        private List<AudioSource> _audioSourcesList = new List<AudioSource>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            var obj = new GameObject("SoundManager");
            obj.AddComponent<SoundManager>();
            DontDestroyOnLoad(obj);
        }

        /// <summary>
        /// このクラスのインスタンスを取得
        /// </summary>
        public static SoundManager Instance
        {
            get
            {
                return instance;
            }
        }
        private static SoundManager instance = null;

        /// <summary>
        /// Start()の実行より先行して処理したい内容を記述
        /// </summary>
        void Awake()
        {
            // 初回作成時
            if (instance == null)
            {
                instance = this;
                // シーンをまたいで削除されないように設定
                DontDestroyOnLoad(gameObject);
            }
            // 2個目以降の作成時
            else
            {
                Destroy(gameObject);
            }

            _soundParameter = Resources.Load<SoundParameter>("ScriptableObjects/SoundParameter");
            if (_soundParameter == null)
            {
                Debug.LogAssertion("SoundParameterがScriptableObjects/SoundParameterに見当たりません");
                return;
            }
            var audioSourceParent = this.transform;
            for (var i = 0; i < _soundParameter.soundInformations.Length; ++i)
            {
                var param = _soundParameter.soundInformations[i];
                var clip = Resources.Load<AudioClip>("Sounds/" + param.soundEffect.ToString());
                var obj = new GameObject("Sound_" + param.soundEffect.ToString());
                var audioSource = obj.AddComponent<AudioSource>();
                audioSource.clip = clip;
                obj.transform.SetParent(audioSourceParent);
                _audioSourcesList.Add(audioSource);
            }
        }
        /// <summary>
        /// 指定したSEを再生
        /// </summary>
        /// <param name="soundEffectID"></param>
        public void PlaySE(SoundEffectID soundEffectID)
        {
            var soundInfo = _soundParameter.soundInformations[(int)soundEffectID];
            if (soundInfo.is3DSound)
            {
                Debug.LogAssertion ("2Dサウンドではありません。Play3DSEメソッドを使用してください");
            }
            if (soundInfo.loop)
            {
                SoundPlayer.PlaySE(_audioSourcesList[(int)soundEffectID],  _soundParameter.soundInformations[(int)soundEffectID]);
            }
            else
            {
                SoundPlayer.PlaySEOneTime(_audioSourcesList[(int)soundEffectID], _audioSourcesList[(int)soundEffectID].clip, _soundParameter.soundInformations[(int)soundEffectID].volume);
            }
        }
        /// <summary>
        /// 指定した3DSEの再生 座標はTransformとは限らない(collision.contact[0].point)→Vector3
        /// </summary>
        /// <param name="soundEffectID">種類</param>
        /// <param name="position">座標はTransformとは限らない</param>
        public void Play3DSE(SoundEffectID soundEffectID , Vector3 position)
        {
            var soundInfo = _soundParameter.soundInformations[(int)soundEffectID];
            if (!soundInfo.is3DSound)
            {
                Debug.LogAssertion("3Dサウンドではありません。PlaySEメソッドを使用してください");
            }
            if (soundInfo.loop)
            {
                SoundPlayer.PlaySE(_audioSourcesList[(int)soundEffectID], _soundParameter.soundInformations[(int)soundEffectID]);
            }
            SoundPlayer.Play3DSEOneTime(_audioSourcesList[(int)soundEffectID].clip,position);
        }
        #region テストコード
#if UNITY_EDITOR
        [ContextMenu("TestPlaySound")]
        private void TestPlaySound()
        {
            StartCoroutine(OnTestPlaySound());
        }

        private IEnumerator OnTestPlaySound()
        {
            foreach (SoundEffectID soundID in Enum.GetValues(typeof(SoundEffectID)))
            {
                PlaySE(soundID);
                Debug.Log(soundID);
                float time = 0;
                while (time < 2)
                {
                    if (Input.GetKeyDown(KeyCode.Return))
                    {                     
                        break;//スキップ
                    }
                    time += Time.deltaTime;
                    yield return null;
                }
            }
        }
        [ContextMenu("TestPlaySoundGetNumber")]
        private void TestPlaySoundGetNumber()
        {
            StartCoroutine(OnTestPlaySoundGetNumber());
        }

        private IEnumerator OnTestPlaySoundGetNumber()
        {
            string input = "";
            SoundEffectID soundEffectID = SoundEffectID.battle_start0;
            Debug.Log("番号を入力");
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                   //変換成功したら再生 未完成
                   if( EnumParseMethod.TryParseAndDebugAssertFormat(input, false,out soundEffectID))
                    PlaySE(soundEffectID);
                    input = "";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    input += "0";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    input += "1";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    input += "2";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    input += "3";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    input += "4";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    input += "5";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    input += "6";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    input += "7";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha8))
                {
                    input += "8";
                }
                else if (Input.GetKeyDown(KeyCode.Alpha9))
                {
                    input += "9";
                }
                Debug.Log(input);
                yield return null;
            }
        }
#endif
        #endregion
    }
}
