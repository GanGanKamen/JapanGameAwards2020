using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking
{
    public enum BGMID
    {
        stage_bgm0, stage_bgm1, stage_bgm2,
        title_bgm0, title_bgm1
    }
    [CreateAssetMenu(fileName = "BGMParameter", menuName = "ScriptableObjects/BGMParameter", order = 3)]
    public class BGMParameter : ScriptableObject
    {
        [System.Serializable]
        public struct BGMInformation
        {
            /// <summary>
            /// エディタ表示用
            /// </summary>
            public BGMID bgm;
            /// <summary>
            /// デフォルト音量1 他の音とバランスをとる
            /// </summary>
            [Range(0, 1)] public float volume;
        }
        public BGMInformation[] bGMInformations;
    }
}