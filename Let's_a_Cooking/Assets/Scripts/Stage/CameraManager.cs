using System.Collections;
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
        ///ショット前のカメラの動きで用いる、カメラの回転の中心の座標情報。メインカメラの親オブジェクトが持つ。食材の中心を軸にカメラを回転させる。
        /// </summary>
        Transform _cameraRotateCenter;

        /// <summary> 0 == top, 1 == front, 2 == side </summary>
        [SerializeField]
        int camNo = 0;

        [SerializeField]
        private CinemachineVirtualCamera topCam;
        [SerializeField]
        private CinemachineVirtualCamera frontCam;
        [SerializeField]
        private CinemachineVirtualCamera sideCam;

        [SerializeField]
        private Vector2 clickPos;   //クリックされた座標の定義
        [SerializeField]
        private Vector3 newTopCameraPos;   //topカメラの座標を定義
        [SerializeField]
        private Vector3 newSideCameraPos;   //sideカメラの座標を定義
        [SerializeField]
        private float wheelScroll;  //マウスホイールのスクロール数を定義

        // Start is called before the first frame update
        void Start()
        {
            _cameraRotateCenter = Camera.main.transform.parent;
        }

        // Update is called once per frame
        void Update()
        {
            BeforeShotCameraRotate();

            //Topカメラの処理
            if(camNo == 0)
            {
                topCam.Priority = 1;
                frontCam.Priority = 0;
                sideCam.Priority = 0;

                //左クリックされた瞬間で呼び出される
                if(Input.GetMouseButtonDown(1))
                {
                    newTopCameraPos = topCam.transform.position;  //現在のカメラの座標を代入
                    clickPos = Input.mousePosition;     //クリックされたマウスの座標を代入
                }
                //左クリックされている間呼び出される
                else if(Input.GetMouseButton(1))
                {
                    newTopCameraPos.x += (clickPos.x - Input.mousePosition.x) / 100;   //x座標のマウスの移動量を計算
                    newTopCameraPos.z += (clickPos.y - Input.mousePosition.y) / 100;   //y座標のマウスの移動量を計算

                    topCam.transform.position = newTopCameraPos;   //マウスの移動量/100を代入
                    clickPos = Input.mousePosition;     //移動後の座標で初期化
                }

                wheelScroll = Input.GetAxis("Mouse ScrollWheel");   //マウスホイールの回転量を格納
                //マウスホイールが入力されたら
                if (wheelScroll != 0)
                {
                    newTopCameraPos.y = topCam.transform.position.y + wheelScroll * -8;
                    topCam.transform.position = newTopCameraPos;       //カメラの座標に代入
                }
            }

            //Frntカメラの処理
            if(camNo == 1)
            {
                topCam.Priority = 0;
                frontCam.Priority = 1;
                sideCam.Priority = 0;
            }

            //Sideカメラの処理
            if(camNo == 2)
            {
                topCam.Priority = 0;
                frontCam.Priority = 0;
                sideCam.Priority = 1;

                //左クリックされた瞬間で呼び出される
                if (Input.GetMouseButtonDown(1))
                {
                    newSideCameraPos = sideCam.transform.position;  //現在のカメラの座標を代入
                    clickPos = Input.mousePosition;     //クリックされたマウスの座標を代入
                }
                //左クリックされている間呼び出される
                else if (Input.GetMouseButton(1))
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
        }

        /// <summary>
        /// ショット前のカメラの動く。食材の中心を軸にカメラを回転させる。
        /// ショット前のカメラの回転は、ショットの向きを決めるオブジェクトのRotationのyを参照し (左右)。x(高さ方向の回転)は参照しない。
        /// ショット前のカメラ動作はmainカメラで行う。
        /// </summary>
        private void BeforeShotCameraRotate()
        {
            var tempShotRotation = ShotManager.Instance.transform.eulerAngles;
            var tempRortation = _cameraRotateCenter.eulerAngles;
            tempRortation.y = tempShotRotation.y;
            _cameraRotateCenter.eulerAngles = tempRortation;
        }
        /// <summary>
        /// TurnControllerによって呼ばれる。指定された場所にメインカメラを配置する。
        /// </summary>
        /// <param name="transform"></param>
        public void CameraPositionResetOnTurnReset(Transform transform)
        {
            _cameraRotateCenter.position = transform.position;
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
    }
}
