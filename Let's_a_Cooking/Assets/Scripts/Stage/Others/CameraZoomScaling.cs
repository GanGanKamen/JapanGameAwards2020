using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// カメラの拡大縮小
    /// </summary>
    public class CameraZoomScaling : MonoBehaviour
    {
        /// <summary>
        /// Androidフラグ
        /// </summary>
        static readonly bool IsAndroid = Application.platform == RuntimePlatform.Android;
        /// <summary>
        /// iOSフラグ
        /// </summary>
        static readonly bool IsIOS = Application.platform == RuntimePlatform.IPhonePlayer;
        /// <summary>
        /// PCフラグ
        /// </summary> 
        static readonly bool IsPC = !IsAndroid && !IsIOS;
        /// <summary>
        /// ズーム開始時のタッチ間の距離
        /// </summary>
        private static float _touchDistanceOnZoomStart = 0;
        public static float GetCameraZoomScalingValue()
        {
            if (IsPC)
            {
                return -8 * Input.GetAxis("Mouse ScrollWheel");   //マウスホイールの回転量を格納
            }
            else
            {
                if (Input.touchCount >= 2)
                {
                    // タッチしている２点を取得
                    Touch touch1 = Input.GetTouch(0);
                    Touch touch2 = Input.GetTouch(1);
                    //2本目の指が触れた瞬間にスタート
                    if (touch2.phase == TouchPhase.Began)
                    {
                        _touchDistanceOnZoomStart = Vector2.Distance(touch1.position, touch2.position);
                    }
                    if (touch1.phase == TouchPhase.Stationary && touch2.phase == TouchPhase.Stationary)
                    {
                        _touchDistanceOnZoomStart = Vector2.Distance(touch1.position, touch2.position);
                    }
                    if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
                    {
                        // タッチ位置の移動後、長さを再測し、前回の距離からの相対値を取る
                        float newTouchDistance = Vector2.Distance(touch1.position, touch2.position);
                        //差分計算
                        float deltaDistance = _touchDistanceOnZoomStart - newTouchDistance;
                        // 1フレーム前の値として記憶
                        _touchDistanceOnZoomStart = newTouchDistance;
                        return deltaDistance / 100.0f;
                    }
                }
                return 0;
            }
        }
    }
}