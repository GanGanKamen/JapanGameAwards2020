using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Touches;

namespace Cooking.Stage
{
    /// <summary>
    /// ターン開始時にアクティブなプレイヤーの情報を取得し、そのプレイヤーを追従
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        /// <summary>
        ///ショット前のカメラの動きで用いる、カメラの回転中心の座標情報。frontカメラの親オブジェクトが持つ。食材の中心を軸にカメラを回転させる
        /// </summary>
        [SerializeField] private Transform _cameraRotateCenter;
        /// <summary>
        ///ショット前のカメラの動きで用いる、すべてのカメラの親オブジェクトの座標情報。位置情報をリセットするときに使う
        /// </summary>
        [SerializeField] private Transform _cameraObjectsTransform;
        private float _changeTopCameraTimeCounter;
        [SerializeField] private float _changeTopCameraTime = 0.3f;
        Vector3[] _cameraLocalPositions;
        /// <summary>
        /// ゲーム開始前のUIにて、ドラッグした状態 = タッチしていた状態でゲームが始まると、ドラッグ量が座標に代入されてバグるのを防ぐ
        /// </summary>
        bool _isTouchOnGamePlay;
        /// <summary> 0 == top, 1 == front, 2 == side </summary>
        public int camNo = 0;

        enum CameraMode
        {
            Wait,
            Top,
            Front,
            Side
        }
        CameraMode cameraMode = CameraMode.Wait;

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
            _cameraLocalPositions = new Vector3[System.Enum.GetValues(typeof(CameraMode)).Length];
            _cameraLocalPositions[(int)CameraMode.Top] = (topCam.transform.localPosition);
            _cameraLocalPositions[(int)CameraMode.Front] = (frontCam.transform.localPosition);
            _cameraLocalPositions[(int)CameraMode.Side] = (sideCam.transform.localPosition);
        }

        // Update is called once per frame
        void Update()
        {
            switch (cameraMode)
            {
                case CameraMode.Wait:
                    break;
                case CameraMode.Top:
                    {
                        topCam.Priority = 1;
                        frontCam.Priority = 0;
                        sideCam.Priority = 0;
                        if (_isTouchOnGamePlay)
                        {
                            //左クリックされている間呼び出される
                            var touchPosition = TouchInput.GetDeltaPosition();
                            newTopCameraPos = topCam.transform.position;
                            newTopCameraPos.x -= (touchPosition.x) / 100;   //x座標のマウスの移動量を計算
                            newTopCameraPos.z -= (touchPosition.y) / 100;   //y座標のマウスの移動量を計算
                            topCam.transform.position = newTopCameraPos;   //マウスの移動量/100を代入
                        }
                        //入力準備ができてからの入力であることを示す変数に代入
                        else if (TouchInput.GetTouchPhase() == TouchInfo.Down)
                        {
                            _isTouchOnGamePlay = true;
                        }

                        wheelScroll = Input.GetAxis("Mouse ScrollWheel");   //マウスホイールの回転量を格納
                                                                            //マウスホイールが入力されたら
                        if (wheelScroll != 0)
                        {
                            newTopCameraPos = topCam.transform.position;  //現在のカメラの座標を代入
                            newTopCameraPos.y = topCam.transform.position.y + wheelScroll * -8;
                            topCam.transform.position = newTopCameraPos;       //カメラの座標に代入
                        }
                    }
                    break;
                case CameraMode.Front:
                    {
                        topCam.Priority = 0;
                        frontCam.Priority = 1;
                        sideCam.Priority = 0;
                    }
                    break;
                case CameraMode.Side:
                    {
                        topCam.Priority = 0;
                        frontCam.Priority = 0;
                        sideCam.Priority = 1;

                        //左クリックされている間呼び出される
                        var touchPosition = TouchInput.GetDeltaPosition();
                        sideCam.transform.position -= transform.forward * (touchPosition.x) / 100;   //マウスの移動量/100をカメラの左右方向に代入
                        sideCam.transform.position -= transform.up * (touchPosition.y) / 100;   //マウスの移動量/100をカメラの上下方向に代入
                        wheelScroll = Input.GetAxis("Mouse ScrollWheel");   //マウスホイールの回転量を格納
                                                                            //マウスホイールが入力されたら
                        if (wheelScroll != 0)
                        {
                            sideCam.transform.position += transform.right * wheelScroll * -4;       //マウスホイールの回転をカメラの前後方向に代入
                        }

                    }
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
                            cameraMode = CameraMode.Top;
                            topCam.LookAt = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].transform;
                            _changeTopCameraTimeCounter = 0;
                        }
                        else
                            _changeTopCameraTimeCounter += Time.deltaTime;
                    }
                    break;
                case ShotState.ShotEndMode:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// EndMode切り替えをUpdateで行い、そのあとのフレームで確実に実行
        /// </summary>
        private void LateUpdate()
        {
            switch (StageSceneManager.Instance.FoodStateOnGameProperty)
            {
                case StageSceneManager.FoodStateOnGame.ShotEnd:
                    CameraTrackReset();
                    break;
            }
        }
        /// <summary>
        /// ショット前のカメラ。食材の中心を軸にカメラを回転させる。
        /// ショット前のカメラの回転は、ショットの向きを決めるオブジェクトのRotationのyを参照し (左右)。x(高さ方向の回転)は参照しない。
        /// </summary>
        private void BeforeShotCameraRotate()
        {
            var tempShotRotation = ShotManager.Instance.transform.eulerAngles;
            var tempRortation = _cameraRotateCenter.eulerAngles;
            tempRortation.y = tempShotRotation.y;
            _cameraRotateCenter.eulerAngles = tempRortation;
        }
        public void OnTop()
        {
            //Topカメラに切り替える
            cameraMode = CameraMode.Top;
        }
        public void OnFront()
        {
            //Frontカメラに切り替える
            cameraMode = CameraMode.Front;
        }
        public void OnSide()
        {
            //Sideカメラに切り替える
            cameraMode = CameraMode.Side;
        }
        /// <summary>
        /// カメラの位置を基準(_cameraMoveCenter)に対する元の位置に戻す
        /// </summary>
        private void SetCameraLocalPosition()
        {
            topCam.transform.localPosition = _cameraLocalPositions[(int)CameraMode.Top];
            frontCam.transform.localPosition = _cameraLocalPositions[(int)CameraMode.Front];
            sideCam.transform.localPosition = _cameraLocalPositions[(int)CameraMode.Side];
        }
        /// <summary>
        /// ターン開始時にカメラの動きの中心をセット player中心
        /// </summary>
        /// <param name="cameraSetPositon"></param>
        public void SetCameraMoveCenterPosition(Vector3 cameraSetPositon)
        {
            _cameraObjectsTransform.position = cameraSetPositon;
            _cameraRotateCenter.position = cameraSetPositon;
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
