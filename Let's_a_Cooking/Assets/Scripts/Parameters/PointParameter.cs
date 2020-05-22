using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cooking
{
    public enum PointOperator
    {
        Plus , Minus, Multiplication , Division
    }
    /// <summary>
    /// ポイントの種類
    /// </summary>
    public enum GetPointType
    {
        EggBreaked, EggCracked,//卵に亀裂・割れる
        TouchSeasoning, TouchRareSeasoning , TouchBubble,
        FirstWash, FirstTowelTouch,
        FallOffShrimpHead, CutFood,
        GoalWithRareSeasoning,
        SeasoningWashAwayed , RareSeasoningWashAwayed, TouchDirtDish
        //後ろに追加しないとデータがずれる
    }
    [CreateAssetMenu(fileName = "PointParameter", menuName = "ScriptableObjects/PointParameter", order = 1)]
    public class PointParameter : ScriptableObject
    {
        /// <summary>
        /// ポイントと演算記号
        /// </summary>
        [System.Serializable]
        public struct PointInformation
        {
            /// <summary>
            /// エディタ表示用
            /// </summary>
            public GetPointType getPointType;
            public int pointValue;
            [Header("Plus + , Minus - , Multiplication × , Division ÷")]
            public PointOperator pointOperator;
        }
        public PointInformation[] pointInformation;
    }
}
