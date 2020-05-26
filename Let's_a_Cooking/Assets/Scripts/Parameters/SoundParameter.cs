using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Cooking
{
    /// <summary>
    /// 効果音ID
    /// </summary>
    public enum SoundEffectID
    {
        food_jump0, food_jump1, food_jump2, food_collide0, food_collide1,
        point_up, point_down,
        egg_break, egg_app,
        faucet_water,
        shrimp_head, knife_slash,
        ui_button0, ui_button1, ui_button2, ui_cancel0, ui_cancel1, ui_cancel2,
        power_move0, power_move1,
        gamestart0, gamestart1, food_goal,
        battle_start0, battle_start1,
        timeout,
        winner,
        pot_boiling0, pot_boiling1, pan_frying
        //後ろに追加しないとデータがずれる
    }
    [CreateAssetMenu(fileName = "SoundParameter", menuName = "ScriptableObjects/SoundParameter", order = 2)]
    public class SoundParameter : ScriptableObject
    {
        [System.Serializable]
        public struct SoundInformation
        {
            /// <summary>
            /// エディタ表示用
            /// </summary>
            public SoundEffectID soundEffect;
            /// <summary>
            /// デフォルト音量1 他の音とバランスをとる
            /// </summary>
            [Range(0,1)]public float volume;
            public bool is3DSound;
            public bool loop;
        }
        public SoundInformation[] soundInformations;
    }
}
