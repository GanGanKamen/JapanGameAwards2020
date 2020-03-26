﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Cooking.Stage
{
    /// <summary>
    /// ターン開始時にアクティブなプレイヤーの情報を取得し、そのプレイヤーを追従する。
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        /// <summary>
        ///ショット前のカメラの動きで用いる、カメラの回転や動きの中心の座標情報。メインカメラの親オブジェクトが持つ。食材の中心を軸にカメラを回転させる。
        /// </summary>
        private Transform _cameraMoveCenter;
        private float _changeTopCameraTimeCounter;
       [SerializeField] private float _changeTopCameraTime = 0.3f;
        Vector3[] _cameraLocalPositions = new Vector3[3];
        /// <summary>
        /// ゲーム開始前のUIにて、ドラッグした状態 = タッチしていた状態でゲームが始まると、ドラッグ量が座標に代入されてバグるのを防ぐ
        /// </summary>
        bool _isTouchOnGamePlay;
        /// <summary> 0 == top, 1 == front, 2 == side </summary>
        public int camNo = 0;

        [SerializeField]
        private CinemachineVirtualCamera topCam;
        [SerializeField]
        private CinemachineVirtualCamera frontCam;
        [SerializeField]
        private CinemachineVirtualCamera sideCam;

        [SerializeField]
        private Vector2 clickPos;   //クリックされた座標の定義
        //[SerializeField]
        private Vector3 newTopCameraPos;   //topカメラの座標を定義
        //[SerializeField]
        private Vector3 newSideCameraPos;   //sideカメラの座標を定義
        //[SerializeField]
        private float wheelScroll;  //マウスホイールのスクロール数を定義
        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得。
        /// </summary>
        public static CameraManager Instance
        {
            get { return _instance; }
        }
        static CameraManager _instance = null;

        /// <summary>
        /// Start()より先に実行。
        /// </summary>
        private void Awake()
        {
            _instance = this;
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            _cameraMoveCenter = Camera.main.transform.parent;
            _cameraLocalPositions[0] = (topCam.transform.localPosition);
            _cameraLocalPositions[1] = (frontCam.transform.localPosition);
            _cameraLocalPositions[2] = (sideCam.transform.localPosition);
        }

        // Update is called once per frame
        void Update()
        {
            switch (UIManager.Instance.MainUIStateProperty)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.DecideOrder:
                    break;
                case ScreenState.Start:
                    break;
                case ScreenState.AngleMode:
                    //Frntカメラの処理
                    if (camNo == 1)
                    {
                        topCam.Priority = 0;
                        frontCam.Priority = 1;
                        sideCam.Priority = 0;
                    }
                    break;
                case ScreenState.SideMode:
                    //Sideカメラの処理
                    if (camNo == 2)
                    {
                        topCam.Priority = 0;
                        frontCam.Priority = 0;
                        sideCam.Priority = 1;

                        //左クリックされた瞬間で呼び出される
                        if (Input.GetMouseButtonDown(0))
                        {
                            newSideCameraPos = sideCam.transform.position;  //現在のカメラの座標を代入
                            clickPos = Input.mousePosition;     //クリックされたマウスの座標を代入
                        }
                        //左クリックされている間呼び出される
                        else if (Input.GetMouseButton(0))
                        {
                            sideCam.transform.position += transform.forward * (clickPos.x - Input.mousePosition.x) / 100;   //マウスの移動量/100をカメラの左右方向に代入
                            sideCam.transform.position += transform.up * (clickPos.y - Input.mousePosition.y) / 100;   //マウスの移動量/100をカメラの上下方向に代入

                            clickPos = Input.mousePosition;     //移動後の座標で初期化
                        }

                        wheelScroll = Input.GetAxis("Mouse ScrollWheel");   //マウスホイールの回転量を格納
                                                                            //マウスホイールが入力されたら
                        if (wheelScroll != 0)
                        {
                            sideCam.transform.position += transform.right * wheelScroll * -4;       //マウスホイールの回転をカメラの前後方向に代入
                        }
                    }
                    break;
                case ScreenState.LookDownMode:
                    //Topカメラの処理
                    if (camNo == 0)
                    {
                        topCam.Priority = 1;
                        frontCam.Priority = 0;
                        sideCam.Priority = 0;

                        //左クリックされた瞬間で呼び出される
                        if (Input.GetMouseButtonDown(0))
                        {
                            newTopCameraPos = topCam.transform.position;  //現在のカメラの座標を代入
                            clickPos = Input.mousePosition;     //クリックされたマウスの座標を代入
                            _isTouchOnGamePlay = true;
                        }
                        //左クリックされている間呼び出される
                        else if (Input.GetMouseButton(0) && _isTouchOnGamePlay)
                        {
                            newTopCameraPos.x += (clickPos.x - Input.mousePosition.x) / 100;   //x座標のマウスの移動量を計算
                            newTopCameraPos.z += (clickPos.y - Input.mousePosition.y) / 100;   //y座標のマウスの移動量を計算

                            topCam.transform.position = newTopCameraPos;   //マウスの移動量/100を代入
                            clickPos = Input.mousePosition;     //移動後の座標で初期化
                        }

                        wheelScroll = Input.GetAxis("Mouse ScrollWheel");   //マウスホイールの回転量を格納
                                                                            //マウスホイールが入力されたら
                                                                            //if (wheelScroll != 0)
                                                                            //{
                                                                            //newTopCameraPos.y = topCam.transform.position.y + wheelScroll * -8;
                                                                            //topCam.transform.position = newTopCameraPos;       //カメラの座標に代入
                                                                            //}
                    }
                    break;
                case ScreenState.PowerMeterMode:
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
            switch (ShotManager.Instance.ShotModeProperty)
            {
                case ShotState.WaitMode:
                    break;
                case ShotState.AngleMode:
                    BeforeShotCameraRotate();
                    break;
                case ShotState.PowerMeterMode:
                    break;
                case ShotState.ShottingMode:
                    {
                        if (_changeTopCameraTimeCounter > _changeTopCameraTime)
                        {
                            camNo = 0;
                            topCam.LookAt = TurnController.Instance.foodStatuses[TurnController.Instance.ActivePlayerIndex].transform;
                            _changeTopCameraTimeCounter = 0;
                        }
                        else
                            _changeTopCameraTimeCounter += Time.deltaTime;
                    }
                    break;
                case ShotState.ShotEndMode:
                    CameraTrackReset();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// ショット前のカメラの動く。食材の中心を軸にカメラを回転させる。
        /// ショット前のカメラの回転は、ショットの向きを決めるオブジェクトのRotationのyを参照し (左右)。x(高さ方向の回転)は参照しない。
        /// ショット前のカメラ動作はmainカメラで行う。
        /// </summary>
        private void BeforeShotCameraRotate()
        {
            var tempShotRotation = ShotManager.Instance.transform.eulerAngles;
            var tempRortation = _cameraMoveCenter.eulerAngles;
            tempRortation.y = tempShotRotation.y;
            _cameraMoveCenter.eulerAngles = tempRortation;
        }
        public void OnTop()
        {
            //           newTopCameraPos = topCam.transform.position;  //現在のtopカメラの座標を代入
            //Topカメラに切り替える
            camNo = 0;
        }
        public void OnFront()
        {
            //Frontカメラに切り替える
            camNo = 1;
        }
        public void OnSide()
        {
            //            newSideCameraPos = sideCam.transform.position;  //現在のsiideカメラの座標を代入
            //Sideカメラに切り替える
            camNo = 2;
        }
        /// <summary>
        /// カメラの位置を基準(_cameraMoveCenter)に対する元の位置に戻す
        /// </summary>
        private void SetCameraLocalPosition()
        {
            topCam.transform.localPosition = _cameraLocalPositions[0];
            frontCam.transform.localPosition = _cameraLocalPositions[1];
            sideCam.transform.localPosition = _cameraLocalPositions[2];
        }
        /// <summary>
        /// ターン開始時にカメラの動きの中心をセット player中心
        /// </summary>
        /// <param name="cameraSetPositon"></param>
        public void SetCameraMoveCenterPosition(Vector3 cameraSetPositon)
        {
            _cameraMoveCenter.position = cameraSetPositon;
            SetCameraLocalPosition();
        }
        /// <summary>
        /// カメラの照準を元に戻す
        /// </summary>
        private void CameraTrackReset()
        {
            topCam.LookAt = null;
            topCam.transform.eulerAngles = new Vector3(90, 0, 0);
        }
    }
}
