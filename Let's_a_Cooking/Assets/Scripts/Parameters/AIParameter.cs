using UnityEngine;

namespace Cooking.Stage
{
    [CreateAssetMenu(fileName = "AIParameter", menuName = "ScriptableObjects/AIParameter", order = 4)]
    public class AIParameter : ScriptableObject
    {
        readonly private static int arrayLength = System.Enum.GetValues(typeof(LimitValue)).Length;
        [Range(0.5f, 2.0f)]
        [SerializeField, Header("要素0 : Min 要素1 : Max ")]private float[] _hardRandomRange = new float[arrayLength];
        public float[] HardRandomRange
        {
            get { return _hardRandomRange; }
        }
        [Range(0.5f, 2.0f)]
        [SerializeField, Header("要素0 : Min 要素1 : Max ")] private float[] _normalRandomRange = new float[arrayLength];
        public float[] NormalRandomRange
        {
            get { return _normalRandomRange; }
        }
        [Range(0.5f, 2.0f)]
        [SerializeField, Header("要素0 : Min 要素1 : Max ")] private float[] _easyRandomRange = new float[arrayLength];
        public float[] EasyRandomRange
        {
            get { return _easyRandomRange; }
        }
    }
}
