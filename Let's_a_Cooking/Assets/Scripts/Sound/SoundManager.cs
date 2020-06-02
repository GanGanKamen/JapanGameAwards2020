using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Cooking
{
    public class SoundManager : MonoBehaviour
    {
        private SEParameter _sEParameter;
        private BGMParameter _bGMParameter;
        static private AudioSource _bGMAudioSource;
        private List<AudioSource> _sEAudioSourcesList = new List<AudioSource>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var obj = new GameObject("SoundManager");
            obj.AddComponent<SoundManager>();
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
            _sEParameter = Resources.Load<SEParameter>("ScriptableObjects/SEParameter");
            _bGMParameter = Resources.Load<BGMParameter>("ScriptableObjects/BGMParameter");
            if (_sEParameter == null)
            {
                Debug.LogAssertion("SoundParameterがScriptableObjects/SEParameterに見当たりません");
                return;
            }
            if (_bGMParameter == null)
            {
                Debug.LogAssertion("BGMParameterがScriptableObjects/BGMParameterに見当たりません");
                return;
            }
            #region BGMオーディオの登録 → シングルトン
            var bGMAudio = Resources.Load<GameObject>("Sounds/Audio/BGMAudioSource");
            var bGMGameObject = Instantiate(bGMAudio);
            bGMGameObject.name = "BGMAudioSource";
            _bGMAudioSource = bGMGameObject.GetComponent<AudioSource>();
            DontDestroyOnLoad(_bGMAudioSource);
            if (_bGMAudioSource == null)
            {
                Debug.LogFormat("BGMAUdioSourceが見つかりません");
            }
            SceneName sceneName = SceneName.OP;
            sceneName = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(SceneChanger.activeSceneName, true, sceneName);
            ChangeBGMOnSceneChange(sceneName);
            #endregion
            #region SEオーディオの登録→SEの数だけ用意
            var audioSourceParent = this.transform;
            for (var i = 0; i < _sEParameter.soundInformations.Length; ++i)
            {
                var param = _sEParameter.soundInformations[i];
                var clip = Resources.Load<AudioClip>("Sounds/SE/" + param.soundEffect.ToString());
                if (clip == null)
                {
                    Debug.LogFormat("サウンド{0}が見つかりません", param);
                    continue;
                }
                var mixer = Resources.Load<AudioMixerGroup>("Sounds/Audio/GameAudio");
                var mixerGroup = mixer.audioMixer.FindMatchingGroups("Master/SE");
                var obj = new GameObject("Sound_" + param.soundEffect.ToString());
                var audioSource = obj.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.loop = param.loop;
                audioSource.volume = param.volume;
                audioSource.outputAudioMixerGroup = mixerGroup[0];
                obj.transform.SetParent(audioSourceParent);
                _sEAudioSourcesList.Add(audioSource);
            }
            #endregion
        }
        /// <summary>
        /// 指定したSEを再生
        /// </summary>
        /// <param name="soundEffectID"></param>
        public void PlaySE(SoundEffectID soundEffectID)
        {
            var soundInfo = _sEParameter.soundInformations[(int)soundEffectID];
            if (soundInfo.is3DSound)
            {
                Debug.LogFormat("{0}は2Dサウンドではありません。Play3DSEメソッドを使用してください", soundEffectID);
            }
            if (_sEAudioSourcesList[(int)soundEffectID] == null)
            {
                Debug.LogFormat("このサウンド{0}は未設定です。", soundEffectID);
                return;
            }
            if (soundInfo.loop)
            {
                SoundPlayer.PlaySE(_sEAudioSourcesList[(int)soundEffectID], _sEParameter.soundInformations[(int)soundEffectID]);
            }
            else
            {
                SoundPlayer.PlaySEOneTime(_sEAudioSourcesList[(int)soundEffectID], _sEAudioSourcesList[(int)soundEffectID].clip, _sEParameter.soundInformations[(int)soundEffectID].volume);
            }
        }
        /// <summary>
        /// 指定した3DSEの再生 座標はTransformとは限らない(collision.contact[0].point)→Vector3
        /// </summary>
        /// <param name="soundEffectID">種類</param>
        /// <param name="position">座標はTransformとは限らない</param>
        public void Play3DSE(SoundEffectID soundEffectID, Vector3 position)
        {
            var soundInfo = _sEParameter.soundInformations[(int)soundEffectID];
            if (!soundInfo.is3DSound)
            {
                Debug.LogFormat("{0}は3Dサウンドではありません。PlaySEメソッドを使用してください", soundEffectID);
            }
            if (_sEAudioSourcesList[(int)soundEffectID] == null)
            {
                Debug.LogFormat("このサウンド{0}は未設定です。", soundEffectID);
                return;
            }
            //AudioSourceの位置をtransform.positionに同期させる方法でないと、音量の変化を反映できないので変える必要あり
            if (soundInfo.loop)
            {
                SoundPlayer.PlaySE(_sEAudioSourcesList[(int)soundEffectID], _sEParameter.soundInformations[(int)soundEffectID]);
            }
            SoundPlayer.Play3DSEOneTime(_sEAudioSourcesList[(int)soundEffectID].clip, position);
        }
        /// <summary>
        /// BGMAudioSourceにサウンドクリップをセット
        /// </summary>
        private bool LoadBGMAudioClip(BGMID bGMID)
        {
            var audioClip = Resources.Load<AudioClip>("Sounds/BGM/" + bGMID.ToString());
            if (audioClip != null)
            {
                _bGMAudioSource.clip = audioClip;
                _bGMAudioSource.volume = _bGMParameter.bGMInformations[(int)bGMID].volume;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void PlayBGM(BGMID bGMID , AudioSource audioSource)
        {
            if (audioSource == null)
            {
                Debug.LogFormat("このサウンド{0}は未設定です。", bGMID);
                return;
            }
            SoundPlayer.PlayBGM(audioSource);
        }
        /// <summary>
        /// 読み込まれるシーンやステージの情報で再生するBGMを変更する
        /// </summary>
        /// <param name="loadSceneName"></param>
        public void ChangeBGMOnSceneChange(SceneName loadSceneName)
        {
            switch (loadSceneName)
            {
                //Develop用シーンなどその他のシーンはOP曲が流れる
                case SceneName.OP:
                    LoadBGMAudioClip(BGMID.title_bgm0);
                    break;
                case SceneName.Title:
                    LoadBGMAudioClip(BGMID.title_bgm1);
                    break;
                case SceneName.SelectStage:
                    LoadBGMAudioClip(BGMID.stage_bgm1);
                    break;
                case SceneName.PlayScene:
                    //遷移前にセットされるステージナンバーのインデックスによって決める
                    switch (Stage.StageSceneManager.StageNumberIndex)
                    {
                        case 0:
                            LoadBGMAudioClip(BGMID.stage_bgm0);
                            break;
                        case 1:
                            LoadBGMAudioClip(BGMID.stage_bgm1);
                            break;
                        case 2:
                            LoadBGMAudioClip(BGMID.stage_bgm2);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        #region テストコード
#if UNITY_EDITOR
        int _bGMIndex = 0;
        [ContextMenu("TestPlayBGM")]
        private void TestPlayBGM()
        {
            StartCoroutine(OnTestPlayBGM());
        }

        private IEnumerator OnTestPlayBGM()
        {
            foreach (BGMID bGMID in Enum.GetValues(typeof(BGMID)))
            {
                if(!LoadBGMAudioClip(bGMID))
                {
                    Debug.LogFormat("サウンド{0}が見つかりません", bGMID);
                    continue;
                }
                var obj = new GameObject("BGM_" + bGMID.ToString());
                PlayBGM(bGMID , _bGMAudioSource);
                Debug.Log(bGMID);
                float time = 0;
                while (time < 60)
                {
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        _bGMAudioSource.Stop();
                        yield return null;//入力を区切るため
                        break;//スキップ
                    }
                    time += Time.deltaTime;
                    yield return null;
                }
            }
        }

        [ContextMenu("TestPlaySound")]
        private void TestPlaySESound()
        {
            StartCoroutine(OnTestPlaySESound());
        }

        private IEnumerator OnTestPlaySESound()
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
                        _sEAudioSourcesList[(int)soundID].Stop();
                        yield return null;//入力を区切るため
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
            StartCoroutine(OnTestPlaySESoundGetNumber());
        }

        private IEnumerator OnTestPlaySESoundGetNumber()
        {
            string input = "";
            SoundEffectID soundEffectID = SoundEffectID.battle_start0;
            Debug.Log("番号を入力");
            while (true)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    //変換成功したら再生 未完成
                    if (EnumParseMethod.TryParseAndDebugAssertFormat(input, false, out soundEffectID))
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
                //他の入力はリセットされる
                else if (Input.anyKeyDown)
                {
                    input += "";
                }
                Debug.Log(input);
                yield return null;
            }
        }
#endif
        #endregion
    }
}
