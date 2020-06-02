using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    public class OptionParamater:MonoBehaviour
    {
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateInstance()
        {
            var obj = new GameObject("OptionParamater");
            obj.AddComponent<OptionParamater>();
            obj.GetComponent<OptionParamater>().Init();
        }
        
        static public UnityEngine.Audio.AudioMixer gameAudio;
        static public int BGM_Volume;
        static public int SE_Volume;
        static public Language language;

        public static OptionParamater Instance
        {
            get
            {
                return instance;
            }
        }
        private static OptionParamater instance = null;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Init()
        {
            gameAudio = Resources.Load<UnityEngine.Audio.AudioMixer>("Sounds/Audio/GameAudio");
            language = GetSystemLanguage();
            BGM_Volume = 7;
            SE_Volume = 7;
            DontDestroyOnLoad(gameObject);
        }

        public Language GetSystemLanguage()
        {
            var osLan = Application.systemLanguage;
            switch (osLan)
            {
                case SystemLanguage.Japanese:
                    return Language.Japanese;
                case SystemLanguage.Chinese:
                    return Language.Chinese;
                default:
                    return Language.English;
            }
        }

        private void Update()
        {
            Debug.Log(language);
        }
    }
}

