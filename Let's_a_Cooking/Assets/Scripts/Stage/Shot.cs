using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// ショットの方向や大きさを決めるオブジェクトにつけるクラスです。
    /// ショットオブジェクトは一つのみで、各プレイヤーで使いまわします。
    /// このオブジェクトは、
    /// </summary>
    public class Shot : MonoBehaviour
    {
        /// <summary>
        /// ショット中か否かを表します。
        /// </summary>
        public bool IsShot
        {
            get { return isShot;}
            set
            {
                isShot = value;
            }
        }
        private bool isShot;
        /// <summary>
        /// ショットの向きを決める際、入力を格納するための変数です。
        /// </summary>
        float verticalRot, horizontalRot;
        /// <summary>
        /// 1フレーム前のマウスの位置です。
        /// </summary>
        Vector2 lastMousePosition;
        /// <summary>
        /// マウス感度を決めます。
        /// </summary>
        [SerializeField]float verticalMouseSensitivity = 25, horizontallMouseSensitivity = 25;
        /// <summary>
        /// 食材を打つ力を定義します。
        /// </summary>
        public float shotPower;
        /// <summary>
        /// パワー調整時の加減に使うスイッチです。
        /// </summary>
        bool powerUp = true;
        /// <summary>
        /// ショット時に力を加えるためのものです。
        /// </summary>
        public Rigidbody ShotRigidbody
        {
            get { return shotRigidbody; }
            set { shotRigidbody = value; }
        }
        private Rigidbody shotRigidbody;
        // Start is called before the first frame update
        void Start()
        {
            lastMousePosition = Input.mousePosition;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isShot)
            {
                Debug.Log(transform.eulerAngles);
                //左右キーのInput判定
                //Y_Rot = Input.GetAxis("Horizontal");
                //X_Rot = Input.GetAxis("Vertical");
                horizontalRot = Input.mousePosition.x - lastMousePosition.x;
                verticalRot = lastMousePosition.y - Input.mousePosition.y;
                //発射方向の角度の制御に使う変数の定義
                var eulerX = transform.eulerAngles.x + verticalRot / 25;
                var eulerY = transform.eulerAngles.y + horizontalRot / 25;
                //発射方向の角度の制御
                if (eulerX > 0 && eulerX <= 30)
                {
                    eulerX = 0;
                }
                else if (eulerX < 300 && eulerX > 270)
                {
                    eulerX = 300;
                }
                transform.eulerAngles = new Vector3(eulerX, eulerY, 0);
                //transform.eulerAngles = new Vector3(eulerX, transform.eulerAngles.y, transform.eulerAngles.z);
                //左クリック中に呼び出される
                if (Input.GetMouseButton(0))
                {

                    if (shotPower < 5)
                    {
                        powerUp = true;
                        shotPower = 5;
                    }
                    else if (shotPower > 20)
                    {
                        powerUp = false;
                        shotPower = 20;
                    }
                    else
                    {
                        if (powerUp)
                        {
                            shotPower += 30 * Time.deltaTime;
                        }
                        else if (!powerUp)
                        {
                            shotPower -= 30 * Time.deltaTime;
                        }
                    }
                }

                //左クリックされた時に呼び出される
                if (Input.GetMouseButtonUp(0))
                {
                    isShot = true;
                    //食材に力を加える処理
                    var initialSpeedVector = transform.forward * shotPower;
                    shotRigidbody.velocity = initialSpeedVector;
                }
                #region デバッグコードです。
                ///
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    var initialSpeedVector = transform.forward * 20;
                    shotRigidbody.velocity = initialSpeedVector;
                    //isShot = true;
                }
                #endregion

            }
            lastMousePosition = Input.mousePosition;
        }
    }

}
