using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Cooking
{
    public class OptionManager : MonoBehaviour
    {
        [SerializeField] private Button menubutton;
        [SerializeField] private Button menuWindowCloseNutton;
        [SerializeField] private Button backTitleButton;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Text bgmText;
        [SerializeField] private Text seText;
        [SerializeField] private Slider seSlider;
        [SerializeField] private GameObject menuWindow;
        [SerializeField] private Button jpnButton;
        [SerializeField] private Button engButton;
        [SerializeField] private Button chnButton;

        

        // Start is called before the first frame update
        void Start()
        {
            MenuInit();
            Button_Init();
            Setdefult();           
        }

        // Update is called once per frame
        void Update()
        {
            SetSoundVolume(-30);
        }

        public void Setdefult()
        {
            switch (OptionParamater.language)
            {
                case Language.Japanese:
                    LanguageSwitch(0);
                    break;
                case Language.English:
                    LanguageSwitch(1);
                    break;
                case Language.Chinese:
                    LanguageSwitch(2);
                    break;
            }
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

        private void MenuInit()
        {
            bgmSlider.value = OptionParamater.BGM_Volume;
            seSlider.value = OptionParamater.SE_Volume;
            menuWindow.SetActive(false);
            menuWindowCloseNutton.gameObject.SetActive(false);
            menubutton.gameObject.SetActive(true);
        }

        private void Button_Init()
        {
            menubutton.onClick.AddListener(() => MenuOpen());
            menuWindowCloseNutton.onClick.AddListener(() => MenuClose());
            backTitleButton.onClick.AddListener(() => BackToTitle());
            jpnButton.onClick.AddListener(() => LanguageSwitch(0));
            engButton.onClick.AddListener(() => LanguageSwitch(1));
            chnButton.onClick.AddListener(() => LanguageSwitch(2));
        }

        private void BackToTitle()
        {
            MenuInit();
            //シーンの切り替え
        }

        private void MenuOpen()
        {
            /*
            if (...) //プレイヤー操作不可能な場合
            {
                return;
            }
            */
            Stage.StageSceneManager.Instance.OpenOptionMenu();
            menuWindow.SetActive(true);
            menuWindowCloseNutton.gameObject.SetActive(true);
            menubutton.gameObject.SetActive(false);
            /*
            if (GameObject.FindGameObjectWithTag("Player") != null) //Playerの取得
            {
                var player = GameObject.FindGameObjectWithTag("Player").GetComponent<...>();
                //プレイヤー操作不可能にする
            }
            */
        }

        private void MenuClose()
        {
            Stage.StageSceneManager.Instance.CloseOptionMenu();
            menuWindow.SetActive(false);
            menuWindowCloseNutton.gameObject.SetActive(false);
            menubutton.gameObject.SetActive(true);
            /*
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                var player = GameObject.FindGameObjectWithTag("Player").GetComponent<...>();
                //プレイヤー操作可能にする
            }
            */
        }

        private void LanguageSwitch(int lan)
        {
            jpnButton.image.color = Color.white;
            engButton.image.color = Color.white;
            chnButton.image.color = Color.white;

            switch (lan)
            {
                case 0:
                    OptionParamater.language = Language.Japanese;
                    jpnButton.image.color = Color.yellow;
                    break;
                case 1:
                    OptionParamater.language = Language.English;
                    engButton.image.color = Color.yellow;
                    break;
                case 2:
                    OptionParamater.language = Language.Chinese;
                    chnButton.image.color = Color.yellow;
                    break;
            }
        }

        private void SetSoundVolume(int min_dB)
        {
            OptionParamater.BGM_Volume = (int)bgmSlider.value;
            OptionParamater.SE_Volume = (int)seSlider.value;
            bgmText.text = OptionParamater.BGM_Volume.ToString();
            seText.text = OptionParamater.SE_Volume.ToString();

            var volDelta = Mathf.Abs(0 - min_dB) / 9;
            var bgmVol = min_dB + volDelta * OptionParamater.BGM_Volume;
            var seVol = min_dB + volDelta * OptionParamater.SE_Volume;
            if (OptionParamater.BGM_Volume == 0)
            {
                OptionParamater.gameAudio.SetFloat("BGMVol", -80);
            }
            else
            {
                OptionParamater.gameAudio.SetFloat("BGMVol", bgmVol);
            }
            
            if(OptionParamater.SE_Volume == 0)
            {
                OptionParamater.gameAudio.SetFloat("SEVol", -80);
            }
            else
            {
                OptionParamater.gameAudio.SetFloat("SEVol", seVol);
            }
            
        }
    }

}

