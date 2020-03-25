using System;
using System.Collections;
using System.Collections.Generic;
using Touches;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{

    public class UIManager : ChangePowerMeter
	{
        /// <summary>
        /// メインとなるUIの状態の数だけUIを用意
        /// </summary>
        [SerializeField] GameObject[] _stageSceneMainUIs = new GameObject[Enum.GetValues(typeof(ScreenState)).Length];
        /// <summary>
        /// UIの状態が入る
        /// </summary>
        public ScreenState MainUIStateProperty
        {
            get { return _mainUIState; }
        }
        private ScreenState _mainUIState = ScreenState.ChooseFood;
        /// <summary>
        /// 順番決め用ゲージを取得
        /// </summary>
        [SerializeField] private Slider _orderGage;
        /// <summary>
        /// shotPowerゲージを取得
        /// </summary>
        [SerializeField]private Slider _powerGage;
        public RectTransform returnButton;
        float _startTimeCounter;
        float _startTime = 1;
        [SerializeField] private Text _turnNumber;
        [SerializeField] private Text _playerNumber;
        [SerializeField] private Text _point;
        private ScreenState _beforeShotScreenState;
        /// <summary>
        /// 順番決め用最大/最小値
        /// </summary>
        [SerializeField] float _orderMin = 5, _orderMax = 20;
        [SerializeField] float _orderMeterSpeed = 50;
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
            #region デフォルトUIの更新(スコア・プレイヤー番号・ターン数)
            _turnNumber.text = TurnController.Instance.TurnNumber.ToString();
            _playerNumber.text = (TurnController.Instance.ActivePlayerIndex + 1).ToString();
            //_point.text = .ToString();
            #endregion
            switch (_mainUIState)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.DecideOrder:
                    if (TouchInput.GetTouchPhase() == TouchInfo.Down)
                    {
                        ChangeUI("Start");
                    }
                    ChangeShotPower(_orderMin,_orderMax, _orderMeterSpeed);
                    _orderGage.value = Power;
                    break;
                case ScreenState.Start:
                    break;
                case ScreenState.AngleMode:
                    break;
                case ScreenState.SideMode:
                    break;
                case ScreenState.LookDownMode:
                    break;
                case ScreenState.PowerMeterMode:
                    //shotPowerをゲージに反映
                    _powerGage.value = ShotManager.Instance.Power;
                    break;
                case ScreenState.ShottingMode:
                    break;
                case ScreenState.Finish:
                    break;
                case ScreenState.Pause:
                    break;
                default:
                    break;
            }
            //Debug.Log(_mainUIState);
        }
        /// <summary>
        /// 食材を選ぶ際のマウスクリックで呼ばれる
        /// </summary>
        /// <param name="foodName"></param>選ばれた食材の名前
        public void ChooseFood(string foodName)
        {
            ///作成中
            ///選ばれた食材に応じてプレイヤーを生成する。現在TurnControllerに生成の役割がある。

            ChangeUI("DecideOrder");
        }
        /// <summary>
        /// パワーメーターモードに変更。ショット先を決めるモード以外から変更されることは想定していない。
        /// </summary>
        public void StartPowerMeterMode()
        {
            _beforeShotScreenState = _mainUIState;
            ChangeUI("PowerMeterMode");
        }
        /// <summary>
        /// ショット先を決めるモードに戻る。スタート状態・ターン終了状態からも呼び出される、リセットの役割を持つ。
        /// </summary>
        public void ResetUIMode()
        {
            switch (_beforeShotScreenState)
            {
                case ScreenState.AngleMode:
                    ChangeUI("AngleMode");
                    break;
                case ScreenState.SideMode:
                    ChangeUI("SideMode");
                    break;
                case ScreenState.ShottingMode:
                    ChangeUI("LookDownMode");
                    break;
            }
        }
        /// <summary>
        /// ボタンによるUIの切り替えを行うメソッド→ボタンのテキストで判別できるようにしたい、継承も利用していきたい publicボタン privateUI切り替えしっかりと分けたい
        /// </summary>
        /// <param name="afterScreenStateString"></param>文字列で変更後のUIの状態を指定
        public void ChangeUI(string afterScreenStateString)
        {
            _stageSceneMainUIs[(int)_mainUIState].SetActive(false);
            ///enum型へ変換 + 変換失敗時に警告
            Debug.AssertFormat(Enum.TryParse(afterScreenStateString, out _mainUIState), "不適切な値:{0}が入力されました。", afterScreenStateString);
            /// ショットの状態を変更→ShotManagerへ
            switch (_mainUIState)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.DecideOrder:
                    break;
                case ScreenState.Start:
                    StartCoroutine(StartGameUI());
                    break;
                case ScreenState.AngleMode:
                    ShotManager.Instance.ChangeShotState(ShotState.AngleMode);
                    CameraManager.Instance.OnFront();
                    break;
                case ScreenState.SideMode:
                    ShotManager.Instance.ChangeShotState(ShotState.WaitMode);
                    CameraManager.Instance.OnSide();
                    break;
                case ScreenState.LookDownMode:
                    ShotManager.Instance.ChangeShotState(ShotState.WaitMode);
                    CameraManager.Instance.OnTop();
                    break;
                case ScreenState.PowerMeterMode:
                    ShotManager.Instance.ChangeShotState(ShotState.PowerMeterMode);
                    break;
                case ScreenState.ShottingMode:
                    _beforeShotScreenState = ScreenState.ShottingMode;
                    break;
                case ScreenState.Finish:
                    break;
                case ScreenState.Pause:
                    break;
                default:
                    break;
            }
            _stageSceneMainUIs[(int)_mainUIState].SetActive(true);
        }

        IEnumerator StartGameUI()
        {
            yield return new WaitForSeconds(1f);
            ChangeUI("LookDownMode");
        }
    }
}
