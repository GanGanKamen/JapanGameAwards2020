using System.Collections;
using Touches;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{
    enum InitializeChoose
    {
        ChooseFood,ChooseAILevel
    }
    /// <summary>
    /// ボタン関連はButtonController・PlayMode中以外のUIの制御
    /// </summary>
    public class UIManager : ChangePowerMeter
    {
        /// <summary>
        /// 残りターンを表示する際、UICanvasを切り替え
        /// </summary>
        [SerializeField] GameObject _stageUICanvas;
        /// <summary>
        /// メインとなるUIの状態の数だけUIを用意
        /// </summary>
        [SerializeField] GameObject[] _stageSceneMainUIs = null;
        /// <summary>
        /// 現在のUIの状態が入る
        /// </summary>
        public ScreenState MainUIStateProperty
        {
            get { return _mainUIState; }
        }
        /// <summary>現在のUIの状態</summary>
        private ScreenState _mainUIState = ScreenState.InitializeChoose;
        /// <summary>
        /// よく使うため変数化
        /// </summary>
        TurnManager _turnManager = null;
        /// <summary>
        /// 食材選択画面での選択ボタンイメージ。順番はFoodStatus.FoodTypeに準じる
        /// </summary>
        //[SerializeField] private Image[] _chooseFoodImages = null;
        /// <summary>
        /// ゲーム開始にかかる時間
        /// </summary>
        private float _startTime = 1;
        /// <summary>
        /// オプションメニューが出ている間は画面タッチ処理は行わない
        /// </summary>
       [SerializeField] private GameObject _optionMenuWindow = null;

        #region ゲーム初期化用変数
        /// <summary>
        /// ゲーム初期化UI ChooseFood,ChooseAILevel
        /// </summary>
        [SerializeField] private GameObject[] _initializeUis = new GameObject[System.Enum.GetValues(typeof(InitializeChoose)).Length];
        #endregion

        #region 順番決め用変数
        /// <summary>
        /// int版 順番決め用ゲージ
        /// </summary>
        [SerializeField] private Image _orderGagesOfInteger = null;
        /// <summary>
        /// float版 順番決め用ゲージ
        /// </summary>
        [SerializeField] private Slider _orderGage = null;
        /// <summary>
        /// 順番決め用最大/最小値
        /// </summary>
        private float _orderMin = 0, _orderMax = 100;
        /// <summary>
        /// 順番決めで変化させる値
        /// </summary>
        private float _powerMeterValue = 0;
        /// <summary>
        /// 現状400 差の4倍 インスペクタにて
        /// </summary>
        [SerializeField] float _orderMeterSpeed = 50;
        [SerializeField] GameObject[] _playerListOrderPower = null;
        [SerializeField] GameObject[] _isAIListOrderPower = null;
        [SerializeField] Text[] _orderPowerTexts = null;
        /// <summary>
        /// 入力受付しない時間用変数
        /// </summary>
        bool _invalidInputDecideOrder;
        #endregion

        #region プレイ画面用変数 (PlayModeUIスクリプトがメインで担当)
        /// <summary>
        /// ショット画面から画面を戻すときに戻し先を指定
        /// </summary>
        private ScreenState _beforeShotScreenState;
        private PlayModeUI _playModeUI;
        public PlayModeUI PlayModeUI
        {
            get { return _playModeUI ? _playModeUI : FindObjectOfType<PlayModeUI>(); }
        }
        #endregion

        #region ゲーム終了画面用変数
        public enum FinishUIMode
        {
            Finish,
            Score,
            Title
        }
        /// <summary>
        /// FInishUIの状態管理
        /// </summary>
        public FinishUIMode FinishUIModeProperty
        {
            get { return _finishUIMode; }
        }
        private FinishUIMode _finishUIMode = FinishUIMode.Finish;
        private float _finishWaitTIme = 2;
        private float _finishTimeCounter = 0;
        /// <summary>
        /// 0:Finish!! 1:Score (2:Retry 保留)
        /// </summary>
        [SerializeField] GameObject[] _finishBackGroundImages = null;
        [SerializeField] GameObject[] _finishScoreImages = null;
        [SerializeField] Text[] _finishScoreTexts = null;
        [SerializeField] Text _winnerPlayerNumber = null;
        #endregion

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
            _turnManager = TurnManager.Instance;
            _playModeUI = FindObjectOfType<PlayModeUI>();
        }

        // Update is called once per frame
        void Update()
        {
            switch (_mainUIState)
            {
                case ScreenState.InitializeChoose:
                    break;
                case ScreenState.DecideOrder:
                    if (!_invalidInputDecideOrder)
                    {
                        //float スライダー
                        //powerMeterValue = _orderGage.value;
                        //_orderGage.value = ChangeShotPower(_orderMin, _orderMax, _orderMeterSpeed, powerMeterValue);
                        //オプション画面を開いていても変化する
                        _powerMeterValue = ChangeShotPower(_orderMin, _orderMax, _orderMeterSpeed, _powerMeterValue);
                        //int Image
                        _orderGagesOfInteger.sprite = ChoosePowerMeterUIOfInteger(_powerMeterValue / _orderMax);
                        if (!TurnManager.Instance.IsAITurn)
                        {
                            if (!_optionMenuWindow.activeInHierarchy)
                            {
                                if (!PreventTouchInputCollision.Instance.TouchInvalid[(int)PreventTouchInputCollision.ButtonName.OptionButton])
                                {
                                    if (TouchInput.GetTouchPhase() == TouchInfo.Down)
                                    {
                                        _invalidInputDecideOrder = true;
                                        //順番を決める数値を決定
                                        //var orderPower = _turnManager.PlayerDecideOrderValue(_turnManager.ActivePlayerIndex, _orderGage.value);
                                        var orderPower = _turnManager.PlayerDecideOrderValue(_turnManager.ActivePlayerIndex, _powerMeterValue);
                                        var orderPowerText = Mathf.FloorToInt(orderPower / 10) + 1;
                                        if (orderPowerText > 10) orderPowerText = 10;
                                        else if (orderPowerText < 1) orderPowerText = 1;
                                        _orderPowerTexts[_turnManager.ActivePlayerIndex].text = orderPowerText.ToString();
                                        StartCoroutine(WaitOnDecideOrder());
                                    }
                                    else if (Input.GetKeyDown(KeyCode.Space))
                                    {
                                        _invalidInputDecideOrder = true;
                                        //順番を決める数値を決定
                                        //_orderGage.value = 100;
                                        _powerMeterValue = 100;
                                        _orderGagesOfInteger.sprite = ChoosePowerMeterUIOfInteger(_powerMeterValue / _orderMax);
                                        var orderPower = _turnManager.PlayerDecideOrderValue(_turnManager.ActivePlayerIndex, _powerMeterValue);
                                        var orderPowerText = Mathf.FloorToInt(orderPower / 10) + 1;
                                        if (orderPowerText > 10) orderPowerText = 10;
                                        else if (orderPowerText < 1) orderPowerText = 1;
                                        _orderPowerTexts[_turnManager.ActivePlayerIndex].text = orderPowerText.ToString();
                                        StartCoroutine(WaitOnDecideOrder());
                                    }
                                }
                            }
                        }
                    }
                    break;
                case ScreenState.Start:
                    break;
                case ScreenState.FrontMode:
                    break;
                case ScreenState.SideMode:
                    break;
                case ScreenState.LookDownMode:
                    break;
                case ScreenState.ShottingMode:
                    break;
                case ScreenState.Finish:
                    switch (_finishUIMode)
                    {
                        case FinishUIMode.Finish:
                            if (_finishTimeCounter >= _finishWaitTIme)
                            {
                                CameraManager.Instance.WinnerCamera(TurnManager.Instance.FoodStatuses[0]);
                                ChangeFinishUI(FinishUIMode.Score);
                                StageSceneManager.Instance.ComparePlayerPointOnFinish();
                                _finishWaitTIme = 0;
                            }
                            else
                            {
                                _finishTimeCounter += Time.deltaTime;
                            }
                            break;
                        case FinishUIMode.Score:
                            if (TouchInput.GetTouchPhase() == TouchInfo.Down)
                            {
                                _finishUIMode = FinishUIMode.Title;
                                StartCoroutine(BackTitle());
                            }
                            break;
                        case FinishUIMode.Title:
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.Pause:
                    break;
                default:
                    break;
            }
        }
        IEnumerator BackTitle()
        {
            float changeTime = 1.2f;
            Fader.FadeInAndOutBlack(0.8f, 0.7f, 0.2f);
            yield return new WaitForSeconds(changeTime);
            SceneChanger.LoadSelectingScene(SceneName.Title);

        }
        /// <summary>
        /// ゲーム終了時の勝者番号を表示 by controller
        /// </summary>
        public void UpdateWinnerPlayerNumber(int winnerPlayerNumber)
        {
            _winnerPlayerNumber.text = winnerPlayerNumber.ToString();
        }
        /// <summary>
        /// ゲーム終了時UI用
        /// </summary>
        /// <param name="afterChangefinishUIMode"></param>
        private void ChangeFinishUI(FinishUIMode afterChangefinishUIMode)
        {
            SoundManager.Instance.PlaySE(SoundEffectID.winner);
            _finishBackGroundImages[(int)_finishUIMode].SetActive(false);
            _finishUIMode = afterChangefinishUIMode;
            _finishBackGroundImages[(int)_finishUIMode].SetActive(true);
        }
        /// <summary>
        /// ショット先を決めるモードに戻る。スタート状態・ターン終了状態からも呼び出される、リセットの役割を持つ。
        /// </summary>
        public void ResetUIMode()
        {
            switch (_beforeShotScreenState)
            {
                case ScreenState.FrontMode:
                    ChangeUI(_beforeShotScreenState);
                    break;
                case ScreenState.SideMode:
                    ChangeUI(_beforeShotScreenState);
                    break;
                case ScreenState.ShottingMode:
                    _playModeUI.ChangeUIOnTurnStart();
                    break;
            }
        }
        /// <summary>
        /// ボタンによるUIの切り替えを行うメソッド→ボタンのテキストで判別できるようにstringを引数にしている
        /// </summary>
        /// <param name="afterScreenStateString"></param>文字列で変更後のUIの状態を指定
        public void ChangeUI(ScreenState afterScreenStateString)
        {
            //パワーメーターから戻るときに使う
            _beforeShotScreenState = _mainUIState;
            if(_beforeShotScreenState != afterScreenStateString)
                _stageSceneMainUIs[(int)_beforeShotScreenState].SetActive(false);
            _mainUIState = afterScreenStateString;
            // ショットの状態を変更→ShotManagerへ
            switch (_mainUIState)
            {
                //AIの強さ選択 他に初期化処理が追加される可能性を考慮した設計にしたい
                case ScreenState.InitializeChoose:
                    _initializeUis[(int)InitializeChoose.ChooseFood].SetActive(false);
                    _initializeUis[(int)InitializeChoose.ChooseAILevel].SetActive(true);
                    break;
                case ScreenState.DecideOrder:
                    var powermaterAudio = GetComponent<AudioSource>();
                    powermaterAudio.loop = true;
                    powermaterAudio.Play();
                    var playerNumber = 0;
                    for (int i = 0; i < GameManager.Instance.PlayerNumber; i++)
                    {
                        _playerListOrderPower[playerNumber].SetActive(true);
                        playerNumber++;
                    }
                    for (int i = 0; i < GameManager.Instance.ComputerNumber; i++)
                    {
                        DecideOrderPowerOnComputer(playerNumber);
                        playerNumber++;
                    }
                    break;
                case ScreenState.Start:
                    StartCoroutine(GameStartUI());
                    break;
                case ScreenState.FrontMode:
                    ShotManager.Instance.ChangeShotState(ShotState.AngleMode);
                    CameraManager.Instance.ChangeCameraState(CameraMode.Front);
                    break;
                case ScreenState.SideMode:
                    ShotManager.Instance.ChangeShotState(ShotState.WaitMode);
                    CameraManager.Instance.ChangeCameraState(CameraMode.Side);
                    break;
                case ScreenState.LookDownMode:
                    ShotManager.Instance.ChangeShotState(ShotState.WaitMode);
                    CameraManager.Instance.ChangeCameraState(CameraMode.Top);
                    break;
                case ScreenState.ShottingMode:
                    _beforeShotScreenState = ScreenState.ShottingMode;
                    _playModeUI.SetLinesActive(false);
                    if (!_turnManager.IsAITurn)
                        ShotManager.Instance.ShotStart();
                    break;
                case ScreenState.Finish:
                    _finishUIMode = FinishUIMode.Finish;
                    for (int i = 0; i < _turnManager.FoodStatuses.Length; i++)
                    {
                        _finishScoreTexts[i].text = StageSceneManager.Instance.GetSumPlayerPoint(i).ToString();
                        _finishScoreImages[i].SetActive(true);
                    }
                    break;
                case ScreenState.Pause:
                    break;
                default:
                    break;
            }
            _stageSceneMainUIs[(int)_mainUIState].SetActive(true);
        }
        /// <summary>
        /// コンピュータが順番を決めるための数値を決める
        /// </summary>
        /// <param name="playerNumber"></param>
        private void DecideOrderPowerOnComputer(int playerNumber)
        {
            _playerListOrderPower[playerNumber].SetActive(true);
            _isAIListOrderPower[playerNumber].SetActive(true);
            _turnManager.AIDecideOrderValue(playerNumber);
            var orderPowerText = Mathf.FloorToInt(_turnManager.OrderPower[playerNumber] / 10) + 1;
            if (orderPowerText > 10) orderPowerText = 10;
            else if (orderPowerText < 1) orderPowerText = 1;
            _orderPowerTexts[playerNumber].text = orderPowerText.ToString();
        }
        public int GetActivePlayerNumber()
        {
            return _turnManager.PlayerIndexArray[_turnManager.ActivePlayerIndex] + 1;
        }
        /// <summary>
        /// 複数有人プレイヤーがいるときは、何度も呼ばれる想定
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitOnDecideOrder()
        {
            var powermaterAudio = GetComponent<AudioSource>();
            powermaterAudio.Stop();
            powermaterAudio.loop = false;
            SoundManager.Instance.PlaySE(SoundEffectID.gamestart1);
            yield return new WaitForSeconds(1f);
            ///ターンの変更
            _turnManager.ChangeTurn();
            _invalidInputDecideOrder = false;
            if (_turnManager.ActivePlayerIndex == 0)
            {
                ChangeUI(ScreenState.Start);
            }
        }
        IEnumerator GameStartUI()
        {
            //シーンの名前に応じてBGMを変更 ステージごとに再生するBGMを変更
            SoundManager.Instance.ChangeBGMOnSceneName(SceneName.PlayScene);
            yield return new WaitForSeconds(_startTime);
            _playModeUI.ChangeUIOnTurnStart();
        }
        /// <summary>
        /// UI内にあるボタンを取得し、触れるかどうかの状態を変更 オプション中にUIの状態が変わることも考慮
        /// </summary>
        /// <param name="isEnable">触れる true 触れない false</param>
        public void ChangeButtonsEnableOnActiveUI(bool isEnable)
        {
            foreach (var stageSceneMainUI in _stageSceneMainUIs)
            {
                Button[] buttons = stageSceneMainUI.GetComponentsInChildren<Button>();
                foreach (var button in buttons)
                {
                    button.enabled = isEnable;
                }
            }
        }
        /// <summary>
        /// ターンが変わるUIを表示
        /// </summary>
        public void DisplayChangeTurnUI(bool remainingTurnsUIIsActive)
        {
            _stageUICanvas.SetActive(!remainingTurnsUIIsActive);
            _playModeUI.RemainingTurnsUICanvas.SetActive(remainingTurnsUIIsActive);
        }
    }
}
