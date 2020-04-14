using System.Collections;
using System.Collections.Generic;
using Touches;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{
    /// <summary>
    /// ボタン関連はButtonController・PlayMode中以外のUIの制御
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
        TurnManager _turnController;
        /// <summary>
        /// 食材選択画面での選択ボタンイメージ。順番はFoodStatus.FoodTypeに準じる
        /// </summary>
        [SerializeField] private Image[] _chooseFoodImages;
        /// <summary>
        /// 選ばれた食材リスト FoodStatus用のenumへ変換
        /// </summary>
        private string[] _chooseFoodNames;
        /// <summary>
        /// ゲーム開始にかかる時間
        /// </summary>
        private float _startTime = 1;

        #region 順番決め用変数
        /// <summary>
        /// 順番決め用ゲージを取得
        /// </summary>
        [SerializeField] private Slider _orderGage;
        /// <summary>
        /// 順番決め用最大/最小値・スライダーと同期するのを忘れない
        /// </summary>
        private float _orderMin = 0, _orderMax = 100;
        /// <summary>
        /// 現状400 差の4倍 インスペクタにて
        /// </summary>
        [SerializeField] float _orderMeterSpeed = 50;
        [SerializeField] GameObject[] _playerListOrderPower;
        [SerializeField] GameObject[] _isAIListOrderPower;
        [SerializeField] Text[] _orderPowerTexts;
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
            Retry
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
        [SerializeField] GameObject[] _finishBackGroundImages;
        [SerializeField] GameObject[] _finishScoreImages;
        [SerializeField] Text[] _finishScoreTexts;
        [SerializeField] Text _winnerPlayerNumber;
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
            _turnController = TurnManager.Instance;
            _playModeUI = FindObjectOfType<PlayModeUI>();
            _chooseFoodNames = new string[GameManager.Instance.playerNumber + GameManager.Instance.computerNumber];
        }

        // Update is called once per frame
        void Update()
        {
            switch (_mainUIState)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.DecideOrder:
                    if (!_invalidInputDecideOrder)
                    {
                        var powerMeterValue = _orderGage.value;
                        _orderGage.value = ChangeShotPower(_orderMin, _orderMax, _orderMeterSpeed, powerMeterValue);
                        if (!TurnManager.Instance.IsAITurn)
                        {
                            if (TouchInput.GetTouchPhase() == TouchInfo.Down)
                            {
                                _invalidInputDecideOrder = true;
                                //順番を決める数値を決定
                                var orderPower =  _turnController.PlayerDecideOrderValue(_turnController.ActivePlayerIndex, _orderGage.value);
                                _orderPowerTexts[_turnController.ActivePlayerIndex].text = orderPower.ToString("00.00");
                                StartCoroutine(WaitOnDecideOrder());
                            }
                            else if (Input.GetKeyDown(KeyCode.Space))
                            {
                                _invalidInputDecideOrder = true;
                                //順番を決める数値を決定
                                _orderGage.value = 100;
                               var orderPower = _turnController.PlayerDecideOrderValue(_turnController.ActivePlayerIndex, _orderGage.value);
                                _orderPowerTexts[_turnController.ActivePlayerIndex].text = orderPower.ToString("00.00");
                                StartCoroutine(WaitOnDecideOrder());
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
                case ScreenState.PowerMeterMode:
                    break;
                case ScreenState.ShottingMode:

                    break;
                case ScreenState.Finish:
                    switch (_finishUIMode)
                    {
                        case FinishUIMode.Finish:
                            if (_finishTimeCounter >= _finishWaitTIme)
                            {
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
                                _finishUIMode = FinishUIMode.Retry;
                            }
                            break;
                        case FinishUIMode.Retry:
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
            _finishBackGroundImages[(int)_finishUIMode].SetActive(false);
            _finishUIMode = afterChangefinishUIMode;
            _finishBackGroundImages[(int)_finishUIMode].SetActive(true);
        }
        /// <summary>
        /// 食材を選ぶ際のマウスクリックで呼ばれる
        /// </summary>
        /// <param name="foodName"></param>選ばれた食材の名前
        public void ChooseFood(string foodName)
        {
            ///作成中
            ///選ばれた食材に応じてプレイヤーを生成する。現在TurnControllerに生成の役割がある。
            _chooseFoodNames[_turnController.ActivePlayerIndex] = foodName;
            //index++;
            ChangeUI("DecideOrder");
        }
        /// <summary>
        /// ショット先を決めるモードに戻る。スタート状態・ターン終了状態からも呼び出される、リセットの役割を持つ。
        /// </summary>
        public void ResetUIMode()
        {
            switch (_beforeShotScreenState)
            {
                case ScreenState.FrontMode:
                    ChangeUI(_beforeShotScreenState.ToString());
                    break;
                case ScreenState.SideMode:
                    ChangeUI(_beforeShotScreenState.ToString());
                    break;
                case ScreenState.ShottingMode:
                    _playModeUI.ChangeUIOnTurnStart();
                    break;
            }
        }
        /// <summary>
        /// ボタンによるUIの切り替えを行うメソッド→ボタンのテキストで判別できるようにしたい、継承も利用していきたい publicボタン privateUI切り替えしっかりと分けたい
        /// </summary>
        /// <param name="afterScreenStateString"></param>文字列で変更後のUIの状態を指定
        public void ChangeUI(string afterScreenStateString)
        {
            ///パワーメーターから戻るときに使う
            _beforeShotScreenState = _mainUIState;
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
                        DecideOrderPowerOnComputer(playerNumber);
                        playerNumber++;
                    }
                    break;
                case ScreenState.Start:
                    StartCoroutine(GameStartUI());
                    break;
                case ScreenState.FrontMode:
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
                    _finishUIMode = FinishUIMode.Finish;
                    for (int i = 0; i < _turnController.FoodStatuses.Length; i++)
                    {
                        _finishScoreImages[i].SetActive(true);
                        _finishScoreTexts[_turnController.PlayerIndexArray[i]].text = _turnController.FoodStatuses[i].playerPoint.Point.ToString();
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
            _turnController.AIDecideOrderValue(playerNumber);
            _orderPowerTexts[playerNumber].text = _turnController.OrderPower[playerNumber].ToString("00.00");
        }
        public int GetActivePlayerNumber()
        {
            return _turnController.PlayerIndexArray[_turnController.ActivePlayerIndex] + 1;
        }
        /// <summary>
        /// 複数有人プレイヤーがいるときは、何度も呼ばれる想定
        /// </summary>
        /// <returns></returns>
        IEnumerator WaitOnDecideOrder()
        {
            yield return new WaitForSeconds(1f);
            ///ターンの変更
            _turnController.ChangeTurn();
            _invalidInputDecideOrder = false;
            if (_turnController.ActivePlayerIndex == 0)
            {
                ChangeUI("Start");
            }
        }
        IEnumerator GameStartUI()
        {
            yield return new WaitForSeconds(_startTime);
            _playModeUI.ChangeUIOnTurnStart();
        }
    }
}
