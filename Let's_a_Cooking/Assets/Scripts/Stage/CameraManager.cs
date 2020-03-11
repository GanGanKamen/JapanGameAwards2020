using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// ターン開始時にアクティブなプレイヤーの情報を取得し、そのプレイヤーを追従します。
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        /// <summary>
        /// ショット前のカメラの回転は、ショットの向きを決めるオブジェクトのRotationのyを参照します(左右)。x(高さ方向の回転)は参照しません。
        /// ショット前のカメラ動作はmainカメラで行います。
        /// ショットオブジェクトは一つのみで、各プレイヤーで使いまわします。
        /// </summary>
        Shot shotObject;
        /// <summary>
        ///ショット前のカメラの動きで用いる、カメラの回転の中心の座標情報です。メインカメラの親オブジェクトが持っています。食材の中心を軸にカメラを回転させるので、食材の座標です。
        /// </summary>
        Transform cameraRotateCenter;
        // Start is called before the first frame update
        void Start()
        {
            shotObject = FindObjectOfType<Shot>();
            cameraRotateCenter = Camera.main.transform.parent;
        }

        // Update is called once per frame
        void Update()
        {
            BeforeShotCameraRotate();
        }

        /// <summary>
        ///ショット前のカメラの動きです。食材の中心を軸にカメラを回転させます。
        /// </summary>
        private void BeforeShotCameraRotate()
        {
            var tempShotRotation = shotObject.transform.eulerAngles;
            var tempRortation = cameraRotateCenter.eulerAngles;
            tempRortation.y = tempShotRotation.y;
            cameraRotateCenter.eulerAngles = tempRortation;
        }
        /// <summary>
        /// TurnControllerによって呼ばれます。指定された場所にメインカメラを配置します。
        /// </summary>
        /// <param name="transform"></param>
        public void CameraPositionResetOnTurnReset(Transform transform)
        {
            cameraRotateCenter.position = transform.position;
        }
    }
}
