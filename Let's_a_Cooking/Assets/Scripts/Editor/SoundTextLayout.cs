using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace Cooking
{
    [CustomEditor(typeof(SoundParameter))]
    public class SoundTextLayout : Editor
    {
        //OnInspectorGUIでカスタマイズのGUIに変更する
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("登録されているSEの種類 : " + Enum.GetValues(typeof(SoundEffectID)).Length.ToString());
            base.OnInspectorGUI();
        }
    }
}
