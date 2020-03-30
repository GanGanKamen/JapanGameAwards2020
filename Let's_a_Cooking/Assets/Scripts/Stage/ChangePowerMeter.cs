﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }
}

