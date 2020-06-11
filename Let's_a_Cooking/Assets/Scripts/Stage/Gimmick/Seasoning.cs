using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class Seasoning : MonoBehaviour
    {
        /// <summary>
        /// 演出がActiveかどうかでレアかどうかを判定
        /// </summary>
        public GameObject RareEffect
        {
            get { return _rareEffect; }
        }
        [SerializeField] private GameObject _rareEffect;
        /// <summary>
        /// 残り3ターンでレアになるかどうか
        /// </summary>
        //public bool IsRareSeasoning
        //{
        //    get { return _isRareSeasoning; }
        //}
        /// <summary>
        /// 残り3ターンでレアになるかどうか
        /// </summary>
        [SerializeField] private bool _isRareSeasoning;
        /// <summary>
        /// 調味料
        /// </summary>
        [SerializeField] ParticleSystem _seasoningParticleSystem;
        public bool IsEmittingStopped
        {
            get { return _isEmittingStopped; }
        }
        private bool _isEmittingStopped;
        public Transform InstantiateSeasoningArea
        {
            get { return _instantiateSeasoningArea; }
        }
        [SerializeField]private Transform _instantiateSeasoningArea = null;
        /// <summary>
        /// ターンが残り少なくなりレア調味料が出現する際にカメラが切り替わる
        /// </summary>
        [SerializeField] private GameObject _rareSeasoningCamera = null;
        [SerializeField] private Transform _cameraMovePoint = null;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        //void Update()
        //{

        //}
        /// <summary>
        /// ターンが残り少なくなりレア調味料が出現する際にカメラが切り替わる
        /// </summary>
        /// <param name="isActive"></param>
        public void SetActiveRareSeasoningCamera(bool isActive)
        {
            _rareSeasoningCamera.SetActive(isActive);
            if (isActive)
            {
                StartCoroutine(CameraTranslate());
            }
        }
        IEnumerator CameraTranslate()
        {
            var vector = _cameraMovePoint.position - _rareSeasoningCamera.transform.position;
            while (vector .magnitude >= 0.01f)
            {
                var delta = (vector) * Time.deltaTime;
                _rareSeasoningCamera.transform.Translate(delta);
                yield return null;
            }
        }
        /// <summary>
        /// レア調味料エフェクトの出現・消滅を行う
        /// </summary>
        /// <param name="isActive"></param>
        public void ManageRareSeasoning(bool isActive)
        {
            if(_isRareSeasoning)
            _rareEffect.SetActive(isActive);
        }
        /// <summary>
        /// あるプレイヤーのターンが終了時に、調味料のアクティブ状態を変更 調味料を取っていたらオフ 調味料発生乱数を引いたらオン
        /// </summary>
        public void ManageSeasoningActive(bool isActive)
        {
            gameObject.SetActive(isActive);
            if (isActive)
            {
                _isEmittingStopped = false;
            }
            else
            {
                _isEmittingStopped = true;
            }
        }
        private void OnParticleCollision(GameObject other)
        {
            //プレイヤーに触れたとき
            if (other.GetComponent<FoodStatus>() != null && !_isEmittingStopped)
            {
                _seasoningParticleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                if (_rareEffect.activeInHierarchy)
                {
                    _isRareSeasoning = false;
                    _rareEffect.GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmitting);
                }
                _isEmittingStopped = true;
            }
        }
    }
}
