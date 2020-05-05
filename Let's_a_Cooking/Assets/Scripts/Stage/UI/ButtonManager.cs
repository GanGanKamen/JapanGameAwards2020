using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{
    /// <summary>
    /// ボタンを押したときに、呼ばれる専用のクラス ボタンとメソッドを紐づける以上の役割はない
    /// </summary>
    public class ButtonManager : MonoBehaviour
    {
        /// <summary>
        /// ボタンの機能の種類 同じ名前をボタンゲームオブジェクトにつける
        /// </summary>
        enum ButtonName
        {
            None,
            Shrimp,
            Egg,
            Chicken,
            Sausage,
            LookDownMode,
            FrontMode,
            SideMode,
            ShottingMode,
            CancelButton
        }
        private ButtonName _buttonName = ButtonName.None;
        private UIManager _uIManager;
        [SerializeField] private float _shotButtonWaitTime = 1.5f;

        private void Start()
        {
            _uIManager = UIManager.Instance;
        }
        /// <summary>
        /// ボタンを押した際に呼ばれる共通メソッド UIManagerを通して表示画面中のボタンの情報を受け取る
        /// </summary>
        /// <param name="button">押されたボタンの情報</param>
        public void OnTouch(Button button)
        {
            EnumParseMethod.TryParseAndDebugAssertFormat(button.name, true,out _buttonName);
            //アクティブボタン特定のための情報 UIの状態を取得
            switch (_uIManager.MainUIStateProperty)
            {
                case ScreenState.ChooseFood:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.Shrimp:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetChooseFoodNames(_buttonName.ToString());
                            _uIManager.ChangeUI("DecideOrder");
                            break;
                        case ButtonName.Egg:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetChooseFoodNames(_buttonName.ToString());
                            _uIManager.ChangeUI("DecideOrder");
                            break;
                        case ButtonName.Chicken:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetChooseFoodNames(_buttonName.ToString());
                            _uIManager.ChangeUI("DecideOrder");
                            break;
                        case ButtonName.Sausage:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetChooseFoodNames(_buttonName.ToString());
                            _uIManager.ChangeUI("DecideOrder");
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.DecideOrder:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.Start:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.FrontMode:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.LookDownMode:
                            _uIManager.ChangeUI(_buttonName.ToString());
                            break;
                        case ButtonName.SideMode:
                            _uIManager.ChangeUI(_buttonName.ToString());
                            break;
                        case ButtonName.ShottingMode:
                            _uIManager.PlayModeUI.ChangeShotButtonTouched(true);
                            StartCoroutine(ShotButtonWait());
                            //パワーメーターを停止して待機させる
                            ShotManager.Instance.ChangeShotState(ShotState.WaitMode);
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.SideMode:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.LookDownMode:
                            _uIManager.ChangeUI(_buttonName.ToString());
                            break;
                        case ButtonName.FrontMode:
                            _uIManager.ChangeUI(_buttonName.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.LookDownMode:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.FrontMode:
                            _uIManager.ChangeUI(_buttonName.ToString());
                            break;
                        case ButtonName.SideMode:
                            _uIManager.ChangeUI(_buttonName.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.ShottingMode:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.Finish:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.Pause:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.CancelButton:
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }
        IEnumerator ShotButtonWait()
        {
            yield return new WaitForSeconds(_shotButtonWaitTime);
            _uIManager.ChangeUI(_buttonName.ToString());
        }

    }
}
