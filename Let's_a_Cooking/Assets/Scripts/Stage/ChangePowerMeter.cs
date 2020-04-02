using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
<<<<<<< HEAD
    /// パワーメーターの実装
=======
    /// パワーメーター実装用クラス
>>>>>>> 605b3a883f6896e6ef20d67d36eef4970e38728a
    /// </summary>
    public class ChangePowerMeter : MonoBehaviour
    {
        /// <summary>
        /// パワー調整時の加減に使うスイッチ。
        /// </summary>
<<<<<<< HEAD
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
=======
        private bool _powerUp = true;
        /// <summary>
        /// valueが変動 float min 最小値,float max 最大値,float メーター変動率 30ショット時,大きいほど速い
>>>>>>> 605b3a883f6896e6ef20d67d36eef4970e38728a
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="meterChangeRate"></param>
<<<<<<< HEAD
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
=======
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
>>>>>>> 605b3a883f6896e6ef20d67d36eef4970e38728a
                }
            }
        }
    }
}

