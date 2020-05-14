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

        public UnityEngine.Audio.AudioMixer gameAudio;

        static public int BGM_Volume;
        static public int SE_Volume;


        // Start is called before the first frame update
        void Start()
        {
            MenuInit();
            UI_Init();
            DontDestroyOnLoad(gameObject);           
        }

        // Update is called once per frame
        void Update()
        {
            BGM_Volume = (int)bgmSlider.value;
            SE_Volume = (int)seSlider.value;
            bgmText.text = BGM_Volume.ToString();
            seText.text = SE_Volume.ToString();

            var bgmVol = BGM_Volume * 10 - 100;
            var seVol = SE_Volume * 10 - 100;
            gameAudio.SetFloat("BGMVol", bgmVol);
            gameAudio.SetFloat("SEVol", seVol);
        }

        private void MenuInit()
        {
            menuWindow.SetActive(false);
            menuWindowCloseNutton.gameObject.SetActive(false);
            menubutton.gameObject.SetActive(true);
        }

        private void UI_Init()
        {
            menubutton.onClick.AddListener(() => MenuOpen());
            menuWindowCloseNutton.onClick.AddListener(() => MenuClose());
            backTitleButton.onClick.AddListener(() => BackToTitle());
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
        /*
        public void SetBGM(float value)
        {
            var volume = value * 10 - 100;
            gameAudio.SetFloat("BGMVol", volume);
            Debug.Log(volume);
        }

        public void SetSE(float value)
        {
            var volume = value * 10 - 100;
            gameAudio.SetFloat("SEVol", volume);
        }
        */
    }

}

