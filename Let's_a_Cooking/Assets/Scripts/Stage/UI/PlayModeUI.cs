using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Cooking.Stage
{
    /// <summary>
    /// ショット関連Uiの制御
    /// </summary>
    public class PlayModeUI : MonoBehaviour
    {
        #region プレイ画面用変数
        /// <summary>
        /// shotPowerゲージを取得
        /// </summary>
        [SerializeField] Slider _shotPowerGage;
        /// <summary>
        /// AIのターン中はショット開始ボタンは表示しない
        /// </summary>
        [SerializeField] private GameObject[] _shotStartButtons;
        [SerializeField] private GameObject _defaultIsAIImage;
        [SerializeField] private Text _turnNumberText;
        [SerializeField] private Text _playerNumberTextOnPlay;
        [SerializeField] private Text _pointNumberTextOnPlay;
        [SerializeField] private GameObject _falledImage;
        [SerializeField] private GameObject _goalImage;
        #endregion
        /// <summary>
        /// よく使うため変数化
        /// </summary>
        TurnManager _turnManager;

        public void InitializeShotPowerGage(ShotParameter shotParameter)
        {
            ///スライダーの値同期(他も必要に応じて追加)
            _shotPowerGage.maxValue = shotParameter.MaxShotPower;
            _shotPowerGage.minValue = shotParameter.MinShotPower;
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
                    _shotPowerGage.value = ShotManager.Instance.ShotPower;
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
                foreach (var shotStartButton in _shotStartButtons)
                {
                    shotStartButton.SetActive(false);
                }
            }
            ///ショット終了時は見下ろしスタート プレイヤーの時
            else
            {
                UIManager.Instance.ChangeUI("LookDownMode");
                _defaultIsAIImage.SetActive(false);
                foreach (var shotStartButton in _shotStartButtons)
                {
                    shotStartButton.SetActive(true);
                }
            }
        }
    }
}
