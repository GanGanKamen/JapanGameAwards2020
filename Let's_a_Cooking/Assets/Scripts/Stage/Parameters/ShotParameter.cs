using UnityEngine;

namespace Cooking.Stage
{
    [CreateAssetMenu(fileName = "ShotParameter", menuName = "ScriptableObjects/ShotParameter", order = 0)]
    public class ShotParameter : ScriptableObject
    {
        /// <summary>
        /// 初期化時にSliderに代入される
        /// </summary>
        [SerializeField, Header("最大パワー")]
        private float _maxShotPower = 20;
        public float MaxShotPower
        {
            get { return _maxShotPower; }
        }
        /// <summary>
        /// 初期化時にSliderに代入される
        /// </summary>
        [SerializeField, Header("最小パワー")]
        private float _minShotPower = 5;
        public float MinShotPower
        {
            get { return _minShotPower; }
        }
        /// <summary>
        /// 15から90度の間を想定,プロパティを通すことで値をチェック。
        /// </summary>
        [SerializeField , Header("垂直方向回転の限界角度")]
        float _limitVerticalAngle = 85;
        /// <summary>
        /// 15から90度の間を想定。垂直方向回転の限界角度に不本意な値が入らないようsetで値をチェック。
        /// </summary>
        public float LimitVerticalAngle
        {
            get { return _limitVerticalAngle; }
            private set
            {
                if (15 < value && value < 90)
                {
                    _limitVerticalAngle = value;
                }
                else
                {
                    _limitVerticalAngle = 85;
                    Debug.Log("想定外の垂直方向の限界回転角度が設定されました。85度に仮設定");
                }
            }
        }
    }
}
