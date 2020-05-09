﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
    public class Shrimp : MonoBehaviour
    {
        [SerializeField] CapsuleCollider _capsuleCollider = null;
        /// <summary>
        /// このスクリプトに置くかは未定
        /// </summary>
        [SerializeField] Animator _foodAnimator = null;
        #region グラフィック関連変数
        [SerializeField] private GameObject _shrimpHead = null;
        /// <summary>
        /// えびの頭が外れているかどうか
        /// </summary>
        public bool IsHeadFallOff
        {
            get { return _isHeadFallOff; }
        }
        private bool _isHeadFallOff;
        #endregion

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// エビの頭が取れる
        /// </summary>
        public void FallOffShrimpHead()
        {
            //頭が取れているとき、不正に呼ばれてしまったとき用
            if (_isHeadFallOff)
            {
                return;
            }
            _isHeadFallOff = true;
            ChangeComponentInfoOnHeadFallOff();
        }
        /// <summary>
        /// 頭が取れたときにコンポーネント情報を変更
        /// </summary>
        private void ChangeComponentInfoOnHeadFallOff()
        {
            _shrimpHead.transform.parent = null;
            _shrimpHead.AddComponent<Rigidbody>();
            var center = _capsuleCollider.center;
            _capsuleCollider.center = new Vector3(center.x, center.y, -0.1008767f);
            _capsuleCollider.height = 0.3048875f;
        }

        public void AnimationManage(bool isEnable)
        {
            if (isEnable)
            {
                _foodAnimator.enabled = isEnable;
            }
            else
            {
                StartCoroutine(AnimationResetCoroutine());
            }
        }
        /// <summary>
        /// アニメーションがリセットされるまでに、1フレーム待つ必要がある
        /// </summary>
        /// <returns></returns>
        IEnumerator AnimationResetCoroutine()
        {
            var info = _foodAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            _foodAnimator.Play(info, 0, 0.0f);
            yield return null;
            _foodAnimator.enabled = false;
        }
    }
}