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
        [SerializeField , Header("垂直方向回転の最大限界角度")]
        float _limitVerticalMaxAngle = 85;
        /// <summary>
        /// 0から最大角度の間を想定,プロパティを通すことで値をチェック。
        /// </summary>
        [SerializeField , Header("垂直方向回転の最小限界角度")]
        float _limitVerticalMinAngle = 0;

        /// <summary>
        /// 15から90度の間を想定。垂直方向回転の限界角度に不本意な値が入らないようsetで値をチェック。
        /// </summary>
        public float LimitVerticalMaxAngle
        {
            get { return _limitVerticalMaxAngle; }
            private set
            {
                if (15 < value && value < 90)
                {
                    _limitVerticalMaxAngle = value;
                }
                else
                {
                    _limitVerticalMaxAngle = 85;
                    Debug.Log("想定(15から85度の範囲)外の垂直方向の限界最大回転角度が設定されました。85度に仮設定");
                }
            }
        }
        /// <summary>
        /// 15から90度の間を想定。垂直方向回転の限界角度に不本意な値が入らないようsetで値をチェック。
        /// </summary>
        public float LimitVerticalMinAngle
        {
            get { return _limitVerticalMinAngle; }
            private set
            {
                if (0 <= value && value < _limitVerticalMaxAngle)
                {
                    _limitVerticalMinAngle = value;
                }
                else
                {
                    _limitVerticalMinAngle = 0;
                    Debug.Log("想定(0から最大アングル)外の垂直方向の限界最小回転角度が設定されました。0度に仮設定");
                }
            }
        }
    }
}
