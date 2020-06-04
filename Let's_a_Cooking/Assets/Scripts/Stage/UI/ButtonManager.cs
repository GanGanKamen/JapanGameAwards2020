﻿using System;
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
            Shrimp, Egg, Chicken, Sausage,
            Easy, Normal,Hard,
            LookDownMode,FrontMode,SideMode,
            ShottingWaitMode,
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
        /// Buttonの引数において、enumが非対応だったため仕方なくstring使用 ボタンを押した際に呼ばれる共通メソッド UIManagerを通して表示画面中のボタンの情報を受け取る 
        /// </summary>
        /// <param name="button">押されたボタンの情報</param>
        public void OnTouch(Button button)
        {
            EnumParseMethod.TryParseAndDebugAssertFormat(button.name, true,out _buttonName);
            //仮の値 一部のボタンの名前で使用
            ScreenState afterScreenState = ScreenState.InitializeChoose;
            //アクティブボタン特定のための情報 UIの状態を取得
            switch (_uIManager.MainUIStateProperty)
            {
                case ScreenState.InitializeChoose:
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.Shrimp:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetChooseFoodNames(_buttonName.ToString());
                            _uIManager.ChangeUI(ScreenState.InitializeChoose);
                            break;
                        case ButtonName.Egg:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetChooseFoodNames(_buttonName.ToString());
                            _uIManager.ChangeUI(ScreenState.InitializeChoose);
                            break;
                        case ButtonName.Chicken:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetChooseFoodNames(_buttonName.ToString());
                            _uIManager.ChangeUI(ScreenState.InitializeChoose);
                            break;
                        case ButtonName.Sausage:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetChooseFoodNames(_buttonName.ToString());
                            _uIManager.ChangeUI(ScreenState.InitializeChoose);
                            break;
                        case ButtonName.Easy:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetAILevel(_buttonName.ToString());
                            _uIManager.ChangeUI(ScreenState.DecideOrder);
                            break;
                        case ButtonName.Normal:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetAILevel(_buttonName.ToString());
                            _uIManager.ChangeUI(ScreenState.DecideOrder);
                            break;
                        case ButtonName.Hard:
                            //index++; 複数プレイヤー対応→TurnManager
                            StageSceneManager.Instance.SetAILevel(_buttonName.ToString());
                            _uIManager.ChangeUI(ScreenState.DecideOrder);
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
                            //一部のボタンの名前で使用
                            afterScreenState = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(_buttonName.ToString(), true, afterScreenState);
                            _uIManager.ChangeUI(afterScreenState);
                            break;
                        case ButtonName.SideMode:
                            //一部のボタンの名前で使用
                            afterScreenState = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(_buttonName.ToString(), true, afterScreenState);
                            _uIManager.ChangeUI(afterScreenState);
                            break;
                        case ButtonName.ShottingWaitMode:
                            button.enabled = false;
                            //一部のボタンの名前で使用
                            afterScreenState = ScreenState.ShottingMode;
                            // _turnManager.FoodStatuses[_turnManager.ActivePlayerIndex].PlayerAnimatioManage(false);
                            _uIManager.PlayModeUI.ChangeShotButtonTouched(true);
                            StartCoroutine(ShotButtonWait(afterScreenState,button));
                            //パワーメーターを停止して待機させる
                            ShotManager.Instance.ChangeShotState(ShotState.ShottingWaitMode);
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.SideMode:
                    //一部のボタンの名前で使用
                    afterScreenState = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(_buttonName.ToString(), true, afterScreenState);
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.LookDownMode:
                            _uIManager.ChangeUI(afterScreenState);
                            break;
                        case ButtonName.FrontMode:
                            _uIManager.ChangeUI(afterScreenState);
                            break;
                        default:
                            break;
                    }
                    break;
                case ScreenState.LookDownMode:
                    //一部のボタンの名前で使用
                    afterScreenState = EnumParseMethod.TryParseAndDebugAssertFormatAndReturnResult(_buttonName.ToString(), true, afterScreenState);
                    switch (_buttonName)
                    {
                        case ButtonName.None:
                            Debug.LogFormat("変換失敗かボタンがありません。画面：{0}", _uIManager.MainUIStateProperty.ToString());
                            break;
                        case ButtonName.FrontMode:
                            _uIManager.ChangeUI(afterScreenState);
                            break;
                        case ButtonName.SideMode:
                            _uIManager.ChangeUI(afterScreenState);
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
        IEnumerator ShotButtonWait(ScreenState afterScreenState ,Button button)
        {
            _uIManager.PlayModeUI.SetLinesActive(true);
            yield return new WaitForSeconds(_shotButtonWaitTime);
            _uIManager.ChangeUI(afterScreenState);
            button.enabled = true;
        }

    }
}
