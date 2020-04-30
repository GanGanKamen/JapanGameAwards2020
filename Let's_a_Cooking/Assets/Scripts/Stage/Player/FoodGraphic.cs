using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 食材の種類に応じて、調味料などに触れたときの見た目を変更する
    /// </summary>
	public class FoodGraphic : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _skinnedMeshRenderer = null;
        [SerializeField] private Material _RareMaterial = null;
        /// <summary>
        /// レア調味料を持っているか
        /// </summary>
        public bool IsRareSeasoningMaterial
        {
            get { return _isRareSeasoningMaterial; }
        }
        private bool _isRareSeasoningMaterial;

        // Start is called before the first frame update
        void Start()
        {
        }
        // Update is called once per frame
        void Update()
        {
        }
        /// <summary>
        /// マテリアルを指定したものに変更
        /// </summary>
        protected void ChangeMaterial(Material material)
        {
            //既に見た目が変化している場合、変化しない
            if (_skinnedMeshRenderer.material == material)
            {
                return;
            }
            else if (material == _RareMaterial)
            {
                _isRareSeasoningMaterial = true;
                _skinnedMeshRenderer.material = material;
            }
            else
            {
                _isRareSeasoningMaterial = false;
                _skinnedMeshRenderer.material = material;
            }
        }
    }
}

