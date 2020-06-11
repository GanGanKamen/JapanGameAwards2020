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
        public GameObject MenuWindow
        {
            get { return menuWindow; }
        }
        [SerializeField] private GameObject menuWindow;
        [SerializeField] private Button jpnButton;
        [SerializeField] private Button engButton;
        [SerializeField] private Button chnButton;

        [SerializeField] private Button staffButton;
        [SerializeField] private Button staffWindow;

        [SerializeField] private AudioClip openAudio;
        [SerializeField] private AudioClip closeAudio;
        [SerializeField] private AudioClip buttonAudio;
        [SerializeField] private AudioClip overAudio;

        [SerializeField] private Text turnText;
        [SerializeField] private Text[] pointTexts;
        [SerializeField] private bool isTitleScene;

        private AudioSource audioSource;
        [SerializeField] private Cooking.Stage.TurnManager turnManager;
        [SerializeField] private Cooking.Stage.StageSceneManager stageSceneManager;
        public static OptionManager OptionManagerProperty{ get { return _optionManager ; }  }
        private static OptionManager _optionManager = null;
        private void Awake()
        {
            _optionManager = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            turnManager = Cooking.Stage.TurnManager.Instance;
            stageSceneManager = Stage.StageSceneManager.Instance;
            MenuInit();
            Button_Init();
            Setdefult();           
        }

        // Update is called once per frame
        void Update()
        {
            SetSoundVolume(-30);
            TurnText();
            PlayerPointText();
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
            audioSource = GetComponent<AudioSource>();
            bgmSlider.value = OptionParamater.BGM_Volume;
            seSlider.value = OptionParamater.SE_Volume;
            SetSoundVolume(-30);
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
            if (staffButton != null) staffButton.onClick.AddListener(() => StaffWindowOpen());
            if (staffWindow != null) staffWindow.onClick.AddListener(() => StaffWindowClose());
        }

        private void BackToTitle()
        {
            StartCoroutine(LoadSceneCoroutine());
        }

        IEnumerator LoadSceneCoroutine()
        {
            audioSource.PlayOneShot(closeAudio);
            Fader.FadeInAndOut(1.5f, 1.0f, 1.5f);
            yield return new WaitForSeconds(2.0f);
            SceneChanger.LoadSelectingScene(SceneName.Title);
        }

        private void MenuOpen()
        {
            if (stageSceneManager != null) Stage.StageSceneManager.Instance.OpenOptionMenu();
            menuWindow.SetActive(true);
            menuWindowCloseNutton.gameObject.SetActive(true);
            menubutton.gameObject.SetActive(false);
            audioSource.PlayOneShot(openAudio);
        }

        private void MenuClose()
        {
            if (stageSceneManager != null) Stage.StageSceneManager.Instance.CloseOptionMenu();
            menuWindow.SetActive(false);
            menuWindowCloseNutton.gameObject.SetActive(false);
            menubutton.gameObject.SetActive(true);
            audioSource.PlayOneShot(closeAudio);

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

            //audioSource.PlayOneShot(buttonAudio);
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

        private void TurnText()
        {
            if (turnManager == null) return;
            var remainTurn = turnManager.RemainingTurns; //10 - turnManager.TurnNumber;
            switch (OptionParamater.language)
            {
                case Language.Japanese:
                    turnText.text = "仕上がりは" + remainTurn.ToString() + "ターン後"; 
                    break;
                case Language.English:
                    if(remainTurn > 1)
                    {
                        turnText.text = remainTurn.ToString() + "Turns Until The Finish";
                    }
                    else
                    {
                        turnText.text = remainTurn.ToString() + "Turn Until The Finish";
                    }
                    break;
                case Language.Chinese:
                    turnText.text = remainTurn.ToString() + "回合後結束";
                    break;
            }
        }

        private void PlayerPointText()
        {
            if (stageSceneManager == null) return;
            var playersPoints = new int[4];
            for(int i = 0; i < 4; i++)
            {
                playersPoints[i] = stageSceneManager.GetPlayerPoint(i);
                pointTexts[i].text = playersPoints[i].ToString();
            }
        }

        private void StaffWindowOpen()
        {
            if (staffWindow == null) return;
            staffWindow.gameObject.SetActive(true);
            staffButton.gameObject.SetActive(false);
            menuWindowCloseNutton.gameObject.SetActive(false);
            audioSource.PlayOneShot(openAudio);
        }

        private void StaffWindowClose()
        {
            staffWindow.gameObject.SetActive(false);
            staffButton.gameObject.SetActive(true);
            menuWindowCloseNutton.gameObject.SetActive(true);
            audioSource.PlayOneShot(closeAudio);
        }
    }

}

