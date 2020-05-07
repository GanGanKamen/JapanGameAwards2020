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
        /// <summary>
        /// 5/2時点でえびのみSkinnedMeshRenderer
        /// </summary>
        [SerializeField] private SkinnedMeshRenderer _foodSkinnedMeshRenderer = null;
        [SerializeField] private MeshRenderer _foodMeshRenderer = null;
        [SerializeField] protected Material _foodNormalGraphic = null;
        protected virtual void Start()
        {
        }
        /// <summary>
        /// レア調味料を持っているか
        /// </summary>
        public bool IsRareSeasoningMaterial
        {
            get { return _isRareSeasoningMaterial; }
        }
        private bool _isRareSeasoningMaterial;
        /// <summary>
        /// マテリアルを指定したものに変更
        /// </summary>
        protected void ChangeMaterial(Material material , FoodType foodType)
        {
            switch (foodType)
            {
                case FoodType.Shrimp:
                    //既に見た目が変化している場合、変化しない
                    if (_foodSkinnedMeshRenderer.material == material)
                    {
                        return;
                    }
                    else if (material == GimmickManager.Instance.RareMaterial)
                    {
                        _isRareSeasoningMaterial = true;
                        _foodSkinnedMeshRenderer.material = material;
                    }
                    else
                    {
                        _isRareSeasoningMaterial = false;
                        _foodSkinnedMeshRenderer.material = material;
                    }
                    break;
                case FoodType.Egg:
                    //既に見た目が変化している場合、変化しない
                    if (_foodMeshRenderer.material == material)
                    {
                        return;
                    }
                    else if (material == GimmickManager.Instance.RareMaterial)
                    {
                        _isRareSeasoningMaterial = true;
                        _foodMeshRenderer.material = material;
                    }
                    else
                    {
                        _isRareSeasoningMaterial = false;
                        _foodMeshRenderer.material = material;
                    }
                    break;
                case FoodType.Chicken:
                    //既に見た目が変化している場合、変化しない
                    if (_foodMeshRenderer.material == material)
                    {
                        return;
                    }
                    else if (material == GimmickManager.Instance.RareMaterial)
                    {
                        _isRareSeasoningMaterial = true;
                        _foodMeshRenderer.material = material;
                    }
                    else
                    {
                        _isRareSeasoningMaterial = false;
                        _foodMeshRenderer.material = material;
                    }
                    break;
                case FoodType.Sausage:
                    //既に見た目が変化している場合、変化しない
                    if (_foodMeshRenderer.material == material)
                    {
                        return;
                    }
                    else if (material == GimmickManager.Instance.RareMaterial)
                    {
                        _isRareSeasoningMaterial = true;
                        _foodMeshRenderer.material = material;
                    }
                    else
                    {
                        _isRareSeasoningMaterial = false;
                        _foodMeshRenderer.material = material;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}

