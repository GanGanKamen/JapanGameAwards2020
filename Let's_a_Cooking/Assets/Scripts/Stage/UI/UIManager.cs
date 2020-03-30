using System.Collections;
using System.Collections.Generic;
using Touches;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{
    /// <summary>
    /// ボタン関連はButtonController・それ以外のUIの制御
    /// </summary>
    public class UIManager : ChangePowerMeter
	{
        /// <summary>
        /// メインとなるUIの状態の数だけUIを用意
        /// </summary>
        [SerializeField] GameObject[] _stageSceneMainUIs = new GameObject[System.Enum.GetValues(typeof(ScreenState)).Length];
        /// <summary>
        /// 現在のUIの状態が入る
        /// </summary>
        public ScreenState MainUIStateProperty
        {
            get { return _mainUIState; }
        }
        /// <summary>現在のUIの状態</summary>
        private ScreenState _mainUIState = ScreenState.ChooseFood;
        /// <summary>
        /// よく使うため変数化
        /// </summary>
        TurnController turnController;
        /// <summary>
        /// 食材選択画面での選択ボタンイメージ。順番はFoodStatus.FoodTypeに準じる
        /// </summary>
        [SerializeField] private Image[] chooseFoodImages;
        /// <summary>
        /// 順番決め用ゲージを取得
        /// </summary>
        [SerializeField] private Slider _orderGage;
        /// <summary>
        /// 順番決め用最大/最小値・スライダーと同期するのを忘れない
        /// </summary>
        float _orderMin = 0, _orderMax = 100;
        [SerializeField] float _orderMeterSpeed = 50;
        [SerializeField] GameObject[] _playerListOrderPower;
        [SerializeField] GameObject[] _isAIListOrderPower;
        [SerializeField] Text[] _orderPowerTexts;
        bool _invalidInputDecideOrder;
        /// <summary>
        /// shotPowerゲージを取得
        /// </summary>
        [SerializeField] Slider _powerGage;
        public RectTransform returnButton;
        /// <summary>
        /// ゲーム開始にかかる時間
        /// </summary>
        private float _startTime = 1;
        [SerializeField] private Text _turnNumberText;
        [SerializeField] private Text _playerNumberText;
        [SerializeField] private Text _pointText;
        /// <summary>
        /// ショット画面から画面を戻すときに戻し先を指定
        /// </summary>
        private ScreenState _beforeShotScreenState;
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
            ///スライダーの値と同期(他も必要に応じて追加)
            ShotManager.Instance.maxShotPower = _powerGage.maxValue;
            ShotManager.Instance.minShotPower = _powerGage.minValue;
            turnController = TurnController.Instance;
        }

        // Update is called once per frame
        void Update()
		{
            #region デフォルトUIの更新(スコア・プレイヤー番号・ターン数)
            _turnNumberText.text = turnController.TurnNumber.ToString();
            _playerNumberText.text = (turnController.IndexArray[turnController.ActivePlayerIndex] + 1).ToString();
            #endregion
            switch (_mainUIState)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.DecideOrder:
                    if (!_invalidInputDecideOrder)
                    {
                        var powerMeterValue = _orderGage.value;
                        _orderGage.value = ChangeShotPower(_orderMin, _orderMax, _orderMeterSpeed, powerMeterValue);
                        if (TouchInput.GetTouchPhase() == TouchInfo.Down)
                        {
                            _invalidInputDecideOrder = true;
                            ///順番を決める数値を決定
                            turnController.DecideOrderValue(turnController.ActivePlayerIndex, _orderGage.value);
                            _orderPowerTexts[turnController.ActivePlayerIndex].text = turnController.OrderPower[turnController.ActivePlayerIndex].ToString("00.00");
                            StartCoroutine(WaitOnDecideOrder());
                        }
                    }
                    break;
                case ScreenState.Start:
                    break;
                case ScreenState.AngleMode:
                    UpdatePointText();
                    break;
                case ScreenState.SideMode:
                    UpdatePointText();
                    break;
                case ScreenState.LookDownMode:
                    UpdatePointText();
                    break;
                case ScreenState.PowerMeterMode:
                    //shotPowerをゲージに反映
                    _powerGage.value = ShotManager.Instance.ShotPower;
                    break;
                case ScreenState.ShottingMode:
                    UpdatePointText();
                    break;
                case ScreenState.Finish:
                    break;
                case ScreenState.Pause:
                    break;
                default:
                    break;
            }
        }
        private void UpdatePointText()
        {
            _pointText.text = turnController.foodStatuses[turnController.ActivePlayerIndex].playerPoint.Point.ToString();
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
            Debug.AssertFormat(System.Enum.TryParse(afterScreenStateString, out _mainUIState), "不適切な値:{0}が入力されました。", afterScreenStateString);
            /// ショットの状態を変更→ShotManagerへ
            switch (_mainUIState)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.DecideOrder:
                    var playerNumber = 0;
                    for (int i = 0; i < GameManager.Instance.playerNumber; i++)
                    {
                        _playerListOrderPower[playerNumber].SetActive(true);
                        playerNumber++;
                    }
                    for (int i = 0; i < GameManager.Instance.computerNumber; i++)
                    {
                        _playerListOrderPower[playerNumber].SetActive(true);
                        _isAIListOrderPower[playerNumber].SetActive(true);
                        turnController.DecideOrderValue(playerNumber, Random.Range(70.0f, 95.0f));
                        _orderPowerTexts[playerNumber].text = turnController.OrderPower[playerNumber].ToString("00.00");
                        playerNumber++;
                    }
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
        IEnumerator WaitOnDecideOrder()
        {
            yield return new WaitForSeconds(1f);
            ///ターンの変更
            turnController.ChangeTurn();
            _invalidInputDecideOrder = false;
            if (turnController.ActivePlayerIndex == 0)
            {
                ChangeUI("Start");
            }
        }
        IEnumerator StartGameUI()
        {
            yield return new WaitForSeconds(_startTime);
            ChangeUI("LookDownMode");
        }
    }
}
