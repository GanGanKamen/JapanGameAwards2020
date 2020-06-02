using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public enum FoodGraphicState
    {
        Normal , HaveSeasoning , HaveRareSeasoning
    }
    /// <summary>
    /// 食材の種類に応じて、調味料などに触れたときの見た目を変更する
    /// </summary>
	public class FoodGraphic : MonoBehaviour
    {
        /// <summary>
        /// 5/2時点でえびのみSkinnedMeshRenderer
        /// </summary>
        [SerializeField,Header("エビのみ")] private SkinnedMeshRenderer _foodSkinnedMeshRenderer = null;
        /// <summary>
        /// エビ以外はこちら
        /// </summary>
        [SerializeField,Header("たまご中身 鶏肉 ソーセージ")] private MeshRenderer _foodMeshRenderer = null;
        [SerializeField] protected Material foodNormalGraphic = null;
        [SerializeField] private Material _seasoningMaterial = null;
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
        /// 調味料をもっているかどうか
        /// </summary>
        public bool IsSeasoningMaterial
        {
            get { return _isSeasoningMaterial; }
        }
        private bool _isSeasoningMaterial;
        /// <summary>
        /// マテリアルを指定したものに変更
        /// </summary>
        protected void ChangeMaterial(Material material , FoodType foodType)
        {
            var textureList = StageSceneManager.Instance.FoodTextureList;
            switch (foodType)
            {
                case FoodType.Shrimp:
                    //既に見た目が変化している場合、変化しない
                    if (_foodSkinnedMeshRenderer.material.mainTexture == material.mainTexture)
                    {
                        Debug.Log("同じ見た目です");
                        return;
                    }
                    //レア調味料を持っている状態で通常調味料を取っても変化しない
                    //else if (material.mainTexture == textureList.)
                    //{
                    //    return;
                    //}
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
                    //おそらく機能しない
                    else if (material == GimmickManager.Instance.RareMaterial)
                    {
                        _isRareSeasoningMaterial = true;
                        _foodMeshRenderer.material = material;
                    }
                    else
                    {
                        _isRareSeasoningMaterial = false;
                        _isSeasoningMaterial = true;
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
        /// <summary>
        /// 食材の見た目変更で、参照するMeshRendererが変わる その際marterialを引き継ぐ Destroyより前に実行
        /// </summary>
        /// <param name="meshRenderers">最初がプレイヤーとしてアクティブなmeshRenderer</param>
        /// <param name="foodType"></param>
        protected void ChangeMeshRendererCutFood(MeshRenderer[] meshRenderers, FoodType foodType)
        {
            switch (foodType)
            {
                case FoodType.Chicken:
                    //調味料を持つとき、見た目を引き継ぐ
                    if (_foodMeshRenderer.material !=  foodNormalGraphic)
                    {
                        for (int i = 0; i < meshRenderers.Length; i++)
                        {
                            meshRenderers[i].material = _foodMeshRenderer.material;
                        }
                    }
                    break;
                case FoodType.Sausage:
                    //調味料を持つとき、見た目を引き継ぐ
                    if (_foodMeshRenderer.material != foodNormalGraphic)
                    {
                        for (int i = 0; i < meshRenderers.Length; i++)
                        {
                            meshRenderers[i].material = _foodMeshRenderer.sharedMaterial;
                        }
                    }
                    break;
                default:
                    break;
            }
            //最初がプレイヤーとしてアクティブなmeshRenderer
            _foodMeshRenderer = meshRenderers[0];
        }
        /// <summary>
        ///  食材の見た目変更で、参照するMeshRendererが変わる 卵の殻がmarterialを引き継ぐ Destroyより前に実行
        /// </summary>
        /// <param name="meshRenderers"></param>
        /// <param name="eggInsideRenderer"></param>
        protected void ChangeMeshRendererCrackedEgg(GameObject[] shells, MeshRenderer eggInsideRenderer)
        {
            if (_isSeasoningMaterial)
            {
                for (int i = 0; i < shells.Length; i++)
                {
                    var meshRenderer = shells[i].GetComponent<MeshRenderer>();
                    meshRenderer.material = _seasoningMaterial;
                }
            }
            _foodMeshRenderer = eggInsideRenderer;
        }
        /// <summary>
        /// ひびが割れるときに変更
        /// </summary>
        protected void ChangeNormalEggGraphic(Material material)
        {
            foodNormalGraphic = material;
        }
    }
}

