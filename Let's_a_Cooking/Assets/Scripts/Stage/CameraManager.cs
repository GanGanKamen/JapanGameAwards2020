using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // Start is called before the first frame update
        void Start()
        {
            _cameraRotateCenter = Camera.main.transform.parent;
        }

        // Update is called once per frame
        void Update()
        {
            BeforeShotCameraRotate();
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
    }
}
