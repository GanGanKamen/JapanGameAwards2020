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
    [CustomEditor(typeof(PointParameter))]
    public class PointTextLayout : Editor
    {
        //OnInspectorGUIでカスタマイズのGUIに変更する
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("登録されているポイント獲得方法の種類 : " + Enum.GetValues(typeof(GetPointType)).Length.ToString());
            base.OnInspectorGUI();
            EditorGUILayout.LabelField("現状ゴール時レア調味料を持っている場合ポイント全体に対して2倍");
        }
    }

}
