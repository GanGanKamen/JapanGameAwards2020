using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Touches;

namespace Cooking.Stage
{
    public enum CameraMode
    {
        /// <summary>カメラの移動処理がない Waitに入る前の状態をキープ</summary>
        Wait,
        Top,
        Front,
        Side
    }
    /// <summary>
    /// ターン開始時にアクティブなプレイヤーの情報を取得し、そのプレイヤーを追従
    /// </summary>
    public class CameraManager : SingletonInstance<CameraManager>
    {
        /// <summary>
        ///カメラの回転中心の座標情報。frontカメラの親オブジェクトが持つ。食材の中心を軸にカメラを回転させる
        /// </summary>
        [SerializeField] private Transform _cameraRotateCenter = null;
        /// <summary>
        ///すべてのカメラの親オブジェクトの座標情報。位置情報をリセットするときに使う
        /// </summary>
        [SerializeField] private GameObject _cameraObjectParentTransform = null;
        private float _changeTopCameraTimeCounter;
        [SerializeField] private float _changeTopCameraTime = 0.3f;
        Vector3[] _cameraLocalPositions;
        Vector3 _cameraLocalRotation;
        /// <summary>
        /// ゲーム開始前のUIにて、ドラッグした状態 = タッチしていた状態でゲームが始まると、ドラッグ量が座標に代入されてバグるのを防ぐ
        /// </summary>
        bool _isTouchOnGamePlay;
        /// <summary> 0 == top, 1 == front, 2 == side </summary>
        public int camNo = 0;
        CameraMode _cameraMode = CameraMode.Wait;
        /// <summary>
        /// オプション開く前のカメラの状態
        /// </summary>
        CameraMode _beforeOptionCameraMode = CameraMode.Wait;
        /// <summary>
        /// 見下ろしカメラの角度 少し斜めから見る
        /// </summary>
        [SerializeField] float topCameraRotation = 40;
        [SerializeField]
        private CinemachineVirtualCamera topCam = null;
        [SerializeField]
        private CinemachineVirtualCamera frontCam = null;
        [SerializeField]
        private CinemachineVirtualCamera sideCam = null;
        [SerializeField]
        private Vector2 clickPos;   //クリックされた座標の定義
        //[SerializeField]
        private Vector3 newTopCameraPos;   //topカメラの座標を定義
        //[SerializeField]
        private Vector3 newSideCameraPos;   //sideカメラの座標を定義
        //[SerializeField]
        private float zoomScalingValue;  //マウスホイールのスクロール数を定義
        /// <summary>
        /// ズーム縮小俯瞰による最大のプレイヤーからの距離
        /// </summary>
        [SerializeField] private float _zoomLimitYPositionFromPlayerAbove = 50;
        /// <summary>
        /// ズーム縮小横視点による最大のプレイヤーからの距離
        /// </summary>
        // [SerializeField] private float _zoomMaxDistanceFromPlayerSide ;
        // [SerializeField] private float _zoomMinDistanceFromPlayerSide ;
        private Vector3 _beforePosition;
        //[SerializeField] Transform cameraPositionOnShotting;
        /// <summary>
        /// カメラの移動範囲を制限
        /// </summary>
        GameObject _cameraLimitZone = null;
        Vector3[] _cameraLimitPosition;

        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// Start()より先に実行。
        /// </summary>
        protected override void Awake()
        {
            CreateSingletonInstance(this, false);
        }
        #endregion
        // Start is called before the first frame update
        void Start()
        {
            _cameraLocalPositions = new Vector3[System.Enum.GetValues(typeof(CameraMode)).Length];
            _cameraLocalPositions[(int)CameraMode.Top] = (topCam.transform.localPosition);
            _cameraLocalPositions[(int)CameraMode.Front] = (frontCam.transform.localPosition);
            _cameraLocalPositions[(int)CameraMode.Side] = (sideCam.transform.localPosition);
            _cameraLocalRotation = frontCam.transform.localEulerAngles;
            _cameraLimitZone = GameObject.FindGameObjectWithTag(TagList.CameraZone.ToString());
            _cameraLimitZone.SetActive(false);
            var referencePoint = _cameraLimitZone.transform.GetChild(0).position;
            _cameraLimitPosition = new Vector3[] { referencePoint, referencePoint + _cameraLimitZone.transform.localScale };
            _cameraLimitZone.SetActive(false);//消し忘れ防止
        }
        // Update is called once per frame
        void Update()
        {
            switch (_cameraMode)
            {
                case CameraMode.Wait:
                    break;
                case CameraMode.Top:
                    {
                        if (!OptionManager.OptionManagerProperty.MenuWindow.activeInHierarchy)
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
                            //入力準備ができてからの入力であることを示す変数に代入 俯瞰から始まるのでupside限定の処理
                            else if (TouchInput.GetTouchPhase() == TouchInfo.Down)
                            {
                                _isTouchOnGamePlay = true;
                            }
                            zoomScalingValue = CameraZoomScaling.GetCameraZoomScalingValue();   //マウスホイールの回転量を格納
                            if (zoomScalingValue != 0)
                            {
                                var playerPosition = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].transform.position + new Vector3(0, 1, 0); //約プレイヤーの大きさ分加算
                                newTopCameraPos = topCam.transform.position;  //現在のカメラの座標を代入
                                newTopCameraPos.y = topCam.transform.position.y + zoomScalingValue;
                                topCam.transform.position = newTopCameraPos;       //カメラの座標に代入
                                                                                   //下限
                                if (topCam.transform.position.y <= playerPosition.y)
                                {
                                    var topCameraPosition = topCam.transform.position;
                                    topCam.transform.position = new Vector3(topCameraPosition.x, playerPosition.y, topCameraPosition.z);
                                }
                                //上限
                                else if (topCam.transform.position.y >= playerPosition.y + _zoomLimitYPositionFromPlayerAbove)
                                {
                                    var topCameraPosition = topCam.transform.position;
                                    topCam.transform.position = new Vector3(topCameraPosition.x, playerPosition.y + _zoomLimitYPositionFromPlayerAbove, topCameraPosition.z);
                                }
                            }
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
                        if (!OptionManager.OptionManagerProperty.MenuWindow.activeInHierarchy)
                        {
                            topCam.Priority = 0;
                            frontCam.Priority = 0;
                            sideCam.Priority = 1;
                            if (_isTouchOnGamePlay)
                            {
                                //左クリックされている間呼び出される
                                var touchPosition = TouchInput.GetDeltaPosition();
                                newSideCameraPos = sideCam.transform.localPosition;
                                newSideCameraPos.z -= (touchPosition.x) / 100;   //x座標のマウスの移動量を計算
                                newSideCameraPos.y += (touchPosition.y) / 100;   //y座標のマウスの移動量を計算
                                sideCam.transform.localPosition = newSideCameraPos;   //マウスの移動量/100を代入
                            }
                            zoomScalingValue = CameraZoomScaling.GetCameraZoomScalingValue();   //マウスホイールの回転量を格納
                            var direction = _cameraRotateCenter.transform.right;
                            //マウスホイールが入力されたら y z二つの値を加算
                            if (zoomScalingValue != 0)
                            {
                                var playerPosition = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].CenterPoint.transform.position + new Vector3(0, 0, 0); //約プレイヤーの大きさ分加算
                                var sideCamPosition = sideCam.transform.localPosition;      //マウスホイールの回転をカメラの前後方向に代入
                                sideCamPosition.x += zoomScalingValue * -2;
                                //sideCam.transform.localPosition = sideCamPosition;
                                //下限
                                //if (sideCam.transform.position.x <= playerPosition.x )
                                //{
                                //    var sideCameraPosition = sideCam.transform.position;
                                //    sideCam.transform.position = new Vector3(sideCameraPosition.x, playerPosition.y, sideCameraPosition.z);
                                //
                                {
                                    _beforePosition = sideCam.transform.localPosition;
                                    sideCam.transform.localPosition = sideCamPosition;
                                    if (sideCam.transform.localPosition.x < -15)
                                    {
                                        sideCam.transform.localPosition = _beforePosition;
                                    }
                                    else if (sideCam.transform.localPosition.x > -1.8f)
                                    {
                                        sideCam.transform.localPosition = _beforePosition;
                                    }

                                }
                            }
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
                case ShotState.ShottingMode:
                    {
                        if (_changeTopCameraTimeCounter > _changeTopCameraTime)
                        {
                            frontCam.LookAt = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].FoodPositionNotRotate;
                            frontCam.Follow = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].FoodPositionNotRotate;
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
            #region カメラ移動範囲の制限
            //frontCam.transform.position = new Vector3(Mathf.Clamp(frontCam.transform.position.x, _cameraLimitPosition[(int)LimitValue.Min].x, _cameraLimitPosition[(int)LimitValue.Max].x),
            //    frontCam.transform.position.y,
            //    Mathf.Clamp(frontCam.transform.position.z, _cameraLimitPosition[(int)LimitValue.Min].z, _cameraLimitPosition[(int)LimitValue.Max].z));

            topCam.transform.position = new Vector3(Mathf.Clamp(topCam.transform.position.x, _cameraLimitPosition[(int)LimitValue.Min].x, _cameraLimitPosition[(int)LimitValue.Max].x),
                Mathf.Clamp(topCam.transform.position.y, _cameraLimitPosition[(int)LimitValue.Min].y, _cameraLimitPosition[(int)LimitValue.Max].y),
                Mathf.Clamp(topCam.transform.position.z, _cameraLimitPosition[(int)LimitValue.Min].z, _cameraLimitPosition[(int)LimitValue.Max].z));

            sideCam.transform.position = new Vector3(Mathf.Clamp(sideCam.transform.position.x, _cameraLimitPosition[(int)LimitValue.Min].x, _cameraLimitPosition[(int)LimitValue.Max].x),
                Mathf.Clamp(sideCam.transform.position.y, _cameraLimitPosition[(int)LimitValue.Min].y, _cameraLimitPosition[(int)LimitValue.Max].y),
                Mathf.Clamp(sideCam.transform.position.z, _cameraLimitPosition[(int)LimitValue.Min].z, _cameraLimitPosition[(int)LimitValue.Max].z));
            #endregion
        }
        /// <summary>
        /// EndMode切り替えをUpdateで行い、そのあとのフレームで確実に実行
        /// </summary>
        private void LateUpdate()
        {
            switch (UIManager.Instance.MainUIStateProperty)
            {
                case ScreenState.Start:
                    CameraTrackReset();//初期化
                    break;
            }
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
        /// <summary>
        /// カメラモードを変更
        /// </summary>
        /// <param name="afterCameraMode">変更後のカメラの状態</param>
        public void ChangeCameraState(CameraMode afterCameraMode)
        {
            //オプション画面を想定 オプション中にカメラの状態が変わることも想定(ショット中→トップカメラ(ターンスタート))
            _beforeOptionCameraMode = _cameraMode;
            _cameraMode = afterCameraMode;
            switch (afterCameraMode)
            {
                case CameraMode.Wait:
                    break;
                case CameraMode.Top:
                    break;
                case CameraMode.Front:
                    if(TurnManager.Instance.IsAITurn)
                    frontCam.LookAt = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].CenterPoint;
                    break;
                case CameraMode.Side:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// オプションからゲーム画面に戻る時呼ばれる
        /// </summary>
        public void ReturnOptionMode()
        {
            _cameraMode = _beforeOptionCameraMode;
        }
        /// <summary>
        /// ショット中に呼ばれる
        /// </summary>
        public void SetCameraPositionNearPlayer()
        {
            var playerCenterPoint = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex].CenterPoint.gameObject;
            _cameraObjectParentTransform.transform.position = playerCenterPoint.transform.position;
        }
        /// <summary>
        /// カメラの位置を基準(_cameraMoveCenter)に対する元の位置に戻す
        /// </summary>
        public void SetCameraLocalPosition()
        {
            topCam.transform.localPosition = _cameraLocalPositions[(int)CameraMode.Top];
            frontCam.transform.localPosition = _cameraLocalPositions[(int)CameraMode.Front];
            sideCam.transform.localPosition = _cameraLocalPositions[(int)CameraMode.Side];
            frontCam.transform.localEulerAngles = _cameraLocalRotation;
        }
        /// <summary>
        /// ターン開始時にカメラの動きの中心をセット player中心
        /// </summary>
        /// <param name="cameraSetPositon"></param>
        public void SetCameraMoveCenterPosition(Vector3 cameraSetPositon)
        {
            _cameraObjectParentTransform.transform.position = cameraSetPositon;
            _cameraRotateCenter.position = cameraSetPositon;
            SetCameraLocalPosition();
        }
        /// <summary>
        /// カメラの照準を元に戻す
        /// </summary>
        private void CameraTrackReset()
        {
            frontCam.LookAt = null;
            frontCam.Follow = null;
            topCam.transform.eulerAngles = new Vector3(topCameraRotation, 0, 0);
        }
        public void WinnerCamera(FoodStatus foodStatus)
        {
            _cameraMode = CameraMode.Front;
            switch (foodStatus.FoodType)
            {
                case FoodType.Shrimp:
                    foodStatus.PlayerAnimatioManage(true);
                    break;
                case FoodType.Egg:
                    foodStatus.PlayerAnimatioManage(true);
                    break;
                case FoodType.Chicken:
                    foodStatus.PlayerAnimatioManage(true);
                    break;
                case FoodType.Sausage:
                    foodStatus.PlayerAnimatioManage(true);
                    break;
                default:
                    break;
            }
            foodStatus.CenterPoint.transform.position = foodStatus.transform.position;
            frontCam.LookAt = foodStatus.CenterPoint.transform;
            frontCam.Follow = foodStatus.CenterPoint.transform;
            EffectManager.Instance.InstantiateEffect(foodStatus.transform.position + new Vector3(0,1.5f,0), EffectManager.EffectPrefabID.Stars);
        }
    }
}
