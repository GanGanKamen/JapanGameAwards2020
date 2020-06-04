using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
    public class Shrimp : MonoBehaviour
    {
        public BoxCollider shrimpBoxCollider = null;
        [SerializeField] CapsuleCollider capsuleCollider = null;
        /// <summary>
        /// 頭が取れた後のコライダーの大きさをシーン上で確認しながら変更するため
        /// </summary>
        public BoxCollider shrimpFallHeadBoxCollider = null;
        /// <summary>
        /// 頭が取れた後のコライダーの大きさをシーン上で確認しながら変更するため
        /// </summary>
        [SerializeField] CapsuleCollider fallHeadCapsuleCollider = null;
        /// <summary>
        /// このスクリプトに置くかは未定
        /// </summary>
        [SerializeField] Animator _foodAnimator = null;
        /// <summary>
        /// 触れたパーティクルをなくすトリガーエリア
        /// </summary>
        public CapsuleCollider particleKillTriggerArea;
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
            EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Slash).parent = this.transform;
            ChangeComponentInfoOnHeadFallOff();
        }
        /// <summary>
        /// 頭が取れたときにコンポーネント情報を変更
        /// </summary>
        private void ChangeComponentInfoOnHeadFallOff()
        {
            _shrimpHead.transform.parent = null;
            _shrimpHead.AddComponent<Rigidbody>();
            //キッチン＝障害物になる 跳ねる
            _shrimpHead.layer = CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.Kitchen]);
            var center = capsuleCollider.center;
            capsuleCollider.center = new Vector3(center.x, center.y, -0.1008767f);
            capsuleCollider.height = 0.3048875f;
            shrimpBoxCollider.center = new Vector3(0.0003482699f, -0.0001129784f, -0.2698632f);
            shrimpBoxCollider.size = new Vector3(0.08083411f, 0.06928827f, 0.2897553f);
            var isGroundedArea = GetComponent<FoodStatus>()?.IsGroundedArea;
            isGroundedArea.transform.localPosition = new Vector3(0.008f, -0.156f, -0.115f);
            isGroundedArea.transform.localScale = new Vector3(0.04075898f, 0.1261869f, 0.33f);
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
