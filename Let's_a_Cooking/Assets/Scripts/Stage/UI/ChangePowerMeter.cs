using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{
    /// <summary>
    /// パワーメーター実装用クラス
    /// </summary>
    public class ChangePowerMeter : MonoBehaviour
    {
        /// <summary>
        /// パワー調整時の加減に使うスイッチ。
        /// </summary>
        private bool _powerUp = true;
        /// <summary>
        /// valueが変動 float min 最小値,float max 最大値,float メーター変動率 30ショット時,大きいほど速い
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="meterChangeRate"></param>
        protected float ChangeShotPower(float min,float max,float meterChangeRate,float value)
        {
            if (_powerUp)
            {
                value += meterChangeRate * Time.deltaTime;
                if (value > max)
                {
                    _powerUp = false;
                    return max;
                }
                else
                {
                    return value;
                }
            }
            ///下降中
            else
            {
                value -= meterChangeRate * Time.deltaTime;
                if (value < min)
                {
                    _powerUp = true;
                    return min;
                }
                else
                {
                    return value;
                }
            }
        }
        /// <summary>
        /// 9段階に分かれているショットゲージのうちどれを表示するか選ぶ 最大最小パワーに対する比率を渡す
        /// </summary>
        /// <param name="rate">最大最小パワーに対する比率</param>
        /// <returns></returns>
        protected Sprite ChoosePowerMeterUIOfInteger(float rate)
        {
            var shotPowerGageSprites = UIManager.Instance.PlayModeUI.ShotPowerGageSprites;
            // ゲージの画像は0番目に最大パワー 最後に最小パワーが入っているため、逆順で返す
            var shotPowerGageIndex = shotPowerGageSprites.Length - 1 -(int)Mathf.Floor(rate * shotPowerGageSprites.Length);
            //最大パワーのときのみ 8 - 9でマイナスになる → 0番目に修正
            if (shotPowerGageIndex < 0)
            {
                shotPowerGageIndex = 0;
            }
            return shotPowerGageSprites[shotPowerGageIndex];
        }
    }
}

