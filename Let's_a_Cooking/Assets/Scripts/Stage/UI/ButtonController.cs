using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{
    /// <summary>
    /// ボタン自体にアタッチ
    /// </summary>
    public class ButtonController : MonoBehaviour
    {
        enum ButtonName
        {
            None,
            Shrimp,
            Egg,
            Chicken,
            Sausage,
            ChangeToTop,
            ChangeToFront,
            ChangeToSide,
            StartPowerMeter,
            CancelButton
        }
        private ButtonName _buttonName = ButtonName.None;
        /// <summary>
        /// ボタンを押した際に呼ばれる共通メソッド UIManagerを通して表示画面中のボタンの情報を受け取る
        /// </summary>
        public void OnTouch()
        {
            //押されたボタンの情報
            var button = gameObject.GetComponent<Button>();
            ///enum型へ変換 + 変換失敗時に警告
            Debug.AssertFormat(Enum.TryParse(button.name, out _buttonName), "不適切なボタンの名前:{0}が入力されました。", button.name);
            //アクティブボタン特定のための情報 UIの状態を取得
            switch (UIManager.Instance.MainUIStateProperty)
            {
                case ScreenState.ChooseFood:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.Shrimp:
                            break;
                        case ButtonName.Egg:
                            break;
                        case ButtonName.Chicken:
                            break;
                        case ButtonName.Sausage:
                            break;
                    }
                    break;
                case ScreenState.DecideOrder:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.Start:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.AngleMode:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.ChangeToTop:
                            break;
                        case ButtonName.ChangeToFront:
                            break;
                        case ButtonName.ChangeToSide:
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.SideMode:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.ChangeToTop:
                            break;
                        case ButtonName.ChangeToFront:
                            break;
                        case ButtonName.ChangeToSide:
                            break;
                        case ButtonName.StartPowerMeter:
                            break;
                        case ButtonName.CancelButton:
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.LookDownMode:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.ChangeToTop:
                            break;
                        case ButtonName.ChangeToFront:
                            break;
                        case ButtonName.ChangeToSide:
                            break;
                        case ButtonName.CancelButton:
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.PowerMeterMode:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.CancelButton:
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.ShottingMode:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.Finish:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.Pause:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", UIManager.Instance.MainUIStateProperty.ToString());
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
        private static void OnTouch(string objectName)
        {

        }
    }

}
