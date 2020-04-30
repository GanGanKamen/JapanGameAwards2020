using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{
    /// <summary>
    /// ショット関連Uiの制御
    /// </summary>
    public class PlayModeUI : ChangePowerMeter
    {
        #region プレイ画面用変数
        /// <summary>
        /// float版 shotPowerゲージ
        /// </summary>
        [SerializeField] Image _shotPowerGage = null;
        /// <summary>
        /// int版 shotPowerゲージ
        /// </summary>
        [SerializeField]private Image _shotPowerGagesOfInteger = null;
        /// <summary>
        /// 9段階に分かれているショットゲージ AwakeでResourcesフォルダから読み取る
        /// </summary>
        public Sprite[] ShotPowerGageSprites
        {
            get { return _shotPowerGageSprites; }
        }
        private Sprite[] _shotPowerGageSprites = null;
        /// <summary>
        /// AIのターン中はショット開始ボタンは表示しない
        /// </summary>
        [SerializeField] private GameObject[] _shotButtons = null;
        [SerializeField] private GameObject _defaultIsAIImage = null;
        [SerializeField] private Text _turnNumberText = null;
        [SerializeField] private Text _playerNumberTextOnPlay = null;
        [SerializeField] private Text _pointNumberTextOnPlay = null;
        [SerializeField] private GameObject _falledImage = null;
        [SerializeField] private GameObject _goalImage = null;
        #endregion
        /// <summary>
        /// よく使うため変数化
        /// </summary>
        TurnManager _turnManager = null;
        private void Awake()
        {
            _shotPowerGageSprites = Resources.LoadAll<Sprite>("Powermeter");
        }

        // Start is called before the first frame update
        void Start()
        {
            _turnManager = TurnManager.Instance;
        }

        // Update is called once per frame
        void Update()
        {
            #region デフォルトUIの更新(プレイヤー番号・ターン数) スコアの更新はUIの状態を限定
            _turnNumberText.text = _turnManager.TurnNumber.ToString();
            _playerNumberTextOnPlay.text = (_turnManager.GetPlayerNumberFromActivePlayerIndex(_turnManager.ActivePlayerIndex)).ToString();
            #endregion
            //ショット関連UIのみを管理
            switch (UIManager.Instance.MainUIStateProperty)
            {
                case ScreenState.FrontMode:
                    {
                        //最大最小パワーに対して、現在のパワーがどれだけあるかを計算
                        var shotPowerRate = (ShotManager.Instance.ShotPower - ShotManager.Instance.ShotParameter.MinShotPower) / (ShotManager.Instance.ShotParameter.MaxShotPower - ShotManager.Instance.ShotParameter.MinShotPower);
                        //floatバージョン shotPowerをゲージに反映
                        //_shotPowerGage.fillAmount = shotPowerRate;

                        //intバージョン shotPowerをゲージに反映
                        _shotPowerGagesOfInteger.sprite = ChoosePowerMeterUIOfInteger(shotPowerRate);
                    }
                    UpdatePointText();
                    break;
                case ScreenState.SideMode:
                    UpdatePointText();
                    break;
                case ScreenState.LookDownMode:
                    UpdatePointText();
                    break;
                case ScreenState.ShottingMode:
                    UpdatePointText();
                    switch (StageSceneManager.Instance.FoodStateOnGameProperty)
                    {
                        case StageSceneManager.FoodStateOnGame.Normal:
                            _falledImage.SetActive(false);
                            _goalImage.SetActive(false);
                            break;
                        case StageSceneManager.FoodStateOnGame.Falled:
                            _falledImage.SetActive(true);
                            _goalImage.SetActive(false);
                            break;
                        case StageSceneManager.FoodStateOnGame.Goal:
                            _falledImage.SetActive(false);
                            _goalImage.SetActive(true);
                            break;
                        case StageSceneManager.FoodStateOnGame.ShotEnd:
                            _falledImage.SetActive(false);
                            _goalImage.SetActive(false);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// アクティブプレイヤーのプレイヤーポイント取得のため、ターンマネージャーを経由してポイント取得
        /// </summary>
        private void UpdatePointText()
        {
            _pointNumberTextOnPlay.text = _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].PlayerPointProperty.Point.ToString();
        }
        /// <summary>
        /// ターン開始時にAIかどうかをチェックしてUIを切り替える
        /// </summary>
        public void ChangeUIOnTurnStart()
        {
            if (_turnManager.IsAITurn)
            {
                UIManager.Instance.ChangeUI("SideMode");
                _defaultIsAIImage.SetActive(true);
                foreach (var shotStartButton in _shotButtons)
                {
                    shotStartButton.SetActive(false);
                }
            }
            ///ショット終了時は見下ろしスタート プレイヤーの時
            else
            {
                UIManager.Instance.ChangeUI("LookDownMode");
                _defaultIsAIImage.SetActive(false);
                foreach (var shotStartButton in _shotButtons)
                {
                    shotStartButton.SetActive(true);
                }
            }
        }
    }
}
