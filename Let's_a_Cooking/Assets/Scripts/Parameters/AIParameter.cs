using UnityEngine;

namespace Cooking.Stage
{
    [CreateAssetMenu(fileName = "AIParameter", menuName = "ScriptableObjects/AIParameter", order = 3)]
    public class AIParameter : ScriptableObject
    {
        [SerializeField, Header("Hard")]
        private float[] _hardRandomRange = new float[System.Enum.GetValues(typeof(LimitValue)).Length];
        public float[] HardRandomRange
        {
            get { return _hardRandomRange; }
        }
        [SerializeField, Header("Normal")]
        private float[] _normalRandomRange = new float[System.Enum.GetValues(typeof(LimitValue)).Length];
        public float[] NormalRandomRange
        {
            get { return _normalRandomRange; }
        }
        [SerializeField, Header("Easy")]
        private float[] _easyRandomRange = new float[System.Enum.GetValues(typeof(LimitValue)).Length];
        public float[] EasyRandomRange
        {
            get { return _easyRandomRange; }
        }
    }
}
