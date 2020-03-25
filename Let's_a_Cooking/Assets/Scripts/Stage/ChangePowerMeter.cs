using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// パワーメーターの実装
    /// </summary>
    public class ChangePowerMeter : MonoBehaviour
    {
        /// <summary>
        /// パワー調整時の加減に使うスイッチ。
        /// </summary>
        protected bool _powerUp = true;
        /// <summary>
        /// メーターで変動させる値
        /// </summary>
        public float Power
        {
            get { return _power; }
        }
        private float _power;
        /// <summary>
        /// メーターで値変動 float min 最小値,float max 最大値,float メーター変動率 30ショット時,大きいほど速い
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="meterChangeRate"></param>
        protected void ChangeShotPower(float min,float max,float meterChangeRate)
        {
            if (_power < min)
            {
                _powerUp = true;
                _power = min;
            }
            else if (_power > max)
            {
                _powerUp = false;
                _power = max;
            }
            else
            {
                if (_powerUp)
                {
                    _power += meterChangeRate * Time.deltaTime;
                }
                else if (!_powerUp)
                {
                    _power -= meterChangeRate * Time.deltaTime;
                }
            }
        }
    }
}

