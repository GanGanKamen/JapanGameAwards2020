using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class Sausage : MonoBehaviour
    {
        public BoxCollider SausageBoxCollider = null;
        [SerializeField] BoxCollider _cutSausageBoxCollider = null;
        /// <summary>
        /// 通常ソーセージ 切られるとDestroyで切り替わる(複数オブジェクトで構成される)
        /// </summary>
        [SerializeField] private GameObject _sausageNormal = null;
        /// <summary>
        /// カットされた後プレイヤーとして動く本体
        /// </summary>
        [SerializeField] private GameObject _sausageCut = null;
        /// <summary>
        /// 完全にオブジェクトから切リ離されるソーセージ
        /// </summary>
        [SerializeField] private GameObject _sausageCutOffObject = null;
        /// <summary>
        /// 切られた後のMeshRenderer
        /// </summary>
        public MeshRenderer[] CutMeshRenderer
        {
            get { return _cutMeshRenderer; }
        }
        [SerializeField, Header("最初がプレイヤーとしてアクティブなMeshRenderer")] private MeshRenderer[] _cutMeshRenderer = null;
        /// <summary>
        /// ソーセージが切られているかどうか
        /// </summary>
        public bool IsCut
        {
            get { return _isCut; }
        }
        private bool _isCut;
        /// <summary>
        /// ソーセージが切られる
        /// </summary>
        public void CutSausage()
        {
            if (_isCut)
            {
                return;
            }
            _isCut = true;
            // 通常ソーセージ 切られて切り替わる(複数オブジェクトで構成される)
            Destroy(_sausageNormal);
            // 切られたソーセージ本体が出現
            _sausageCut.SetActive(true);
            _sausageCutOffObject.transform.parent = null;
            var rigidbody = _sausageCutOffObject.AddComponent<Rigidbody>();
            rigidbody.mass = 0.01f;
            var isGroundedArea = GetComponent<FoodStatus>()?.IsGroundedArea;
            isGroundedArea.transform.localPosition = new Vector3(-0.016f, -0.07243f, -0.0979f);
            isGroundedArea.transform.localScale = new Vector3(0.04076f, 0.05391148f, 0.1718356f);
            //必要に応じて力を加える プレイヤーから見て邪魔かも？
            //rigidbody.AddForce(Vector3.right * 100);
        }
    }
}
