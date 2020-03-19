using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{

    public class UIManager : MonoBehaviour
	{
        /// <summary>
        /// メインとなるUIの状態の数だけUIを用意
        /// </summary>
        [SerializeField] GameObject[] _stageSceneMainUIs = new GameObject[(int)ScreenState.Finish + 1];
        /// <summary>
        /// UIの状態が入る
        /// </summary>
        public ScreenState MainUIStateProperty
        {
            get { return _mainUIState; }
            set
            {
                _mainUIState = value;
            }
        }
        private ScreenState _mainUIState = ScreenState.ChooseFood;
        /// <summary>
        /// shotPowerゲージを取得
        /// </summary>
        [SerializeField]private Slider _powerGage;
        public RectTransform returnButton;
        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得
        /// </summary>
        public static UIManager Instance
        {
            get { return _instance; }
        }
        static UIManager _instance = null;

        /// <summary>
        /// Start()より先に実行
        /// </summary>
        private void Awake()
        {
            _instance = this;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
		{
        }

        // Update is called once per frame
        void Update()
		{
            switch (_mainUIState)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.Start:
                    break;
                case ScreenState.AngleMode:
                    break;
                case ScreenState.LookDown:
                    break;
                case ScreenState.PowerMeterMode:
                    //shotPowerをゲージに反映
                    _powerGage.value = ShotManager.Instance.ShotPower;
                    break;
                case ScreenState.Shotting:
                    break;
                case ScreenState.Finish:
                    break;
                case ScreenState.Pause:
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 食材を選ぶ際のマウスクリックで呼ばれる
        /// </summary>
        /// <param name="foodName"></param>選ばれた食材の名前
        public void ChooseFood(string foodName)
        {
            ///作成中
            ///選ばれた食材に応じてプレイヤーを生成する。現在TurnControllerに生成の役割がある。


            StartCoroutine(StartGameUI());
        }
        /// <summary>
        /// パワーメーターモードに変更。ショット先を決めるモード以外から変更されることは想定していない。
        /// </summary>
        public void StartPowerMeterMode()
        {
            _stageSceneMainUIs[(int)ScreenState.AngleMode].SetActive(false);
            _stageSceneMainUIs[(int)ScreenState.PowerMeterMode].SetActive(true);
            ShotManager.Instance.ShotModeProperty = ShotState.PowerMeterMode;
            _mainUIState = ScreenState.PowerMeterMode;
        }
        /// <summary>
        /// ショット先を決めるモードに戻る。スタート状態・ターン終了状態からも呼び出される、リセットの役割を持つ。
        /// </summary>
        public void ResetUIMode()
        {
            foreach (var uIState in _stageSceneMainUIs)
            {
                uIState.SetActive(false);
            }
            _stageSceneMainUIs[(int)ScreenState.AngleMode].SetActive(true);
            ShotManager.Instance.ShotModeProperty = ShotState.AngleMode;
            _mainUIState = ScreenState.AngleMode;
        }
        /// <summary>
        /// UIの遷移を一つのメソッドにまとめる予定
        /// </summary>
        /// <param name="beforeScreenState"></param>
        /// <param name="afterScreenState"></param>
        private void ChangeUI(ScreenState beforeScreenState, ScreenState afterScreenState)
        {

        }
        IEnumerator StartGameUI()
        {
            ///ChangeUIメソッドで一つにまとめる予定
            _stageSceneMainUIs[(int)ScreenState.ChooseFood].SetActive(false);
            _stageSceneMainUIs[(int)ScreenState.Start].SetActive(true);
            _mainUIState = ScreenState.Start;
            yield return new WaitForSeconds(1f);
            _stageSceneMainUIs[(int)ScreenState.Start].SetActive(false);
            _stageSceneMainUIs[(int)ScreenState.AngleMode].SetActive(true);
            _mainUIState = ScreenState.AngleMode;
        }
    }

}
