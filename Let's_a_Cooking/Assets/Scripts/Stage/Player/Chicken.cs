using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class Chicken : MonoBehaviour
    {
        public BoxCollider chickenBoxCollider = null;
        /// <summary>
        /// 切れた後のコライダーの大きさをシーン上で確認しながら変更するため
        /// </summary>
        public BoxCollider chickenCutBoxCollider = null;
        [SerializeField] BoxCollider _cutChickenBoxCollider = null;
        /// <summary>
        /// 通常ささみ 切られるとDestroyで切り替わる(複数オブジェクトで構成される)
        /// </summary>
        [SerializeField] private GameObject _chickenNormal = null;
        /// <summary>
        /// カットされた後プレイヤーとして動く本体
        /// </summary>
        [SerializeField] private GameObject _chickenCut = null;
        /// <summary>
        /// 完全にオブジェクトから切リ離されるささみ
        /// </summary>
        [SerializeField] private GameObject _chickenCutOffObject = null;
        /// <summary>
        /// 切られた後のMeshRenderer 0番目がプレイヤーとしてアクティブなMeshRenderer
        /// </summary>
        public MeshRenderer[] CutMeshRenderer
        {
            get { return _cutMeshRenderer; }
        }
        /// <summary>
        /// 0番目がプレイヤーとしてアクティブなMeshRenderer
        /// </summary>
        [SerializeField , Header("最初がプレイヤーとしてアクティブなMeshRenderer")] private MeshRenderer[] _cutMeshRenderer = null;
        /// <summary>
        /// ささみが切られているかどうか
        /// </summary>
        public bool IsCut
        {
            get { return _isCut; }
        }
        private bool _isCut;
       [SerializeField] Material _cutMaterial;
        /// <summary>
        /// ささみが切られる
        /// </summary>
        public void CutChicken(Vector3 cutDirection , float cutPower)
        {
            if (_isCut)
            {
                return;
            }
            _isCut = true;
            // 通常ささみ 切られて切り替わる(複数オブジェクトで構成される)
            Destroy(_chickenNormal);
            // 切られたささみ本体が出現
            _chickenCut.SetActive(true);
            _chickenCutOffObject.transform.parent = null;
            var rigidbody = _chickenCutOffObject.AddComponent<Rigidbody>();
            rigidbody.mass = 1f;
            rigidbody.AddForce( -cutDirection * cutPower, ForceMode.Impulse);
            var isGroundedArea = GetComponent<FoodStatus>()?.IsGroundedArea;
            isGroundedArea.transform.localPosition = new Vector3(0.0079f, -0.156f, 0.1696f);
            isGroundedArea.transform.localScale = new Vector3(0.04076f, 0.12619f, 0.2873457f);
            EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Slash).parent = this.transform;
            //必要に応じて力を加える プレイヤーから見て邪魔かも？
            //rigidbody.AddForce(Vector3.right * 100);
        }
        public void SetTransparentMaterial()
        {
            _cutMeshRenderer[1].material = _cutMaterial;
            _cutMeshRenderer[1].gameObject.layer = CalculateLayerNumber.ChangeSingleLayerNumberFromLayerMask(StageSceneManager.Instance.LayerListProperty[(int)LayerList.FoodCollision]);
        }
    }
}
