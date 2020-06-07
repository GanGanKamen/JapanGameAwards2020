using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public enum FoodGraphicState
    {
        Normal , HaveSeasoning , HaveRareSeasoning
    }
    public enum ShrimpParts
    {
        ///<summary>しっぽは頭が取れた後の本体となる</summary>
        Tail, Head
    }
    /// <summary>
    /// 食材の種類に応じて、調味料などに触れたときの見た目を変更する
    /// </summary>
	public class FoodGraphic : MonoBehaviour
    {
        /// <summary>
        /// 5/2時点でえびのみSkinnedMeshRenderer
        /// </summary>
        [SerializeField,Header("エビのみ")] protected SkinnedMeshRenderer[] foodSkinnedMeshRenderer = null;
        /// <summary>
        /// エビ以外はこちら
        /// </summary>
        [SerializeField,Header("たまご中身 鶏肉 ソーセージ")] protected MeshRenderer _foodMeshRenderer = null;
        [SerializeField] protected Material foodNormalGraphic = null;
        [SerializeField] private Material _seasoningMaterial = null;
        FoodTextureList _foodTextureList = null;
        protected virtual void Start()
        {
            _foodTextureList = StageSceneManager.Instance.FoodTextureList;
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
        protected void ChangeMaterialByWashingFood(FoodType foodType , FoodStatus.Food food)
        {
            switch (foodType)
            {
                case FoodType.Shrimp:
                    if (food.shrimp.IsHeadFallOff)
                    {
                        foodSkinnedMeshRenderer[(int)ShrimpParts.Tail].material = StageSceneManager.Instance.FoodTextureList.normalFoodMaterials[(int)foodType];
                    }
                    else
                    {
                        foodSkinnedMeshRenderer[(int)ShrimpParts.Tail].material = StageSceneManager.Instance.FoodTextureList.normalFoodMaterials[(int)foodType];
                        foodSkinnedMeshRenderer[(int)ShrimpParts.Head].material = StageSceneManager.Instance.FoodTextureList.normalFoodMaterials[System.Enum.GetValues(typeof(FoodType)).Length];//4番目
                    }
                    break;
                case FoodType.Egg:
                    break;
                case FoodType.Chicken:
                    break;
                case FoodType.Sausage:
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// マテリアルを指定した共通のものに変更
        /// </summary>
        protected void ChangeMaterial(Material material , FoodType foodType , FoodStatus.Food food)
        {
            switch (foodType)
            {
                case FoodType.Shrimp:
                    //既に見た目が変化している場合、変化しない
                    if (foodSkinnedMeshRenderer[(int)ShrimpParts.Tail].material.mainTexture == material.mainTexture)
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
                        if (food.shrimp.IsHeadFallOff)
                        {
                            foodSkinnedMeshRenderer[(int)ShrimpParts.Tail].material = material;
                        }
                        else
                        {
                            foreach (var skinnedMeshRenderer in foodSkinnedMeshRenderer)
                            {
                                skinnedMeshRenderer.material = material;
                            }
                        }
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
                        _isSeasoningMaterial = true;
                        if (food.egg.HasBroken)
                        {
                            food.egg.InsideMeshRenderer.material = material;
                        }
                        else
                        {
                            _foodMeshRenderer.material = material;
                        }
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
                        if (food.chicken.IsCut)
                        {
                            food.chicken.CutMeshRenderer[0].material = material;
                        }
                        else
                        {
                            _foodMeshRenderer.material = material;
                        }
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
                        if (food.sausage.IsCut)
                        {
                            food.chicken.CutMeshRenderer[0].material = material;
                        }
                        else
                        {
                            _foodMeshRenderer.material = material;
                        }
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
                    if (_foodMeshRenderer.material.color ==  _foodTextureList.seasoningMaterial.color)
                    {
                        for (int i = 0; i < meshRenderers.Length; i++)
                        {
                            meshRenderers[i].material = _foodTextureList.seasoningMaterial;
                        }
                    }
                    break;
                case FoodType.Sausage:
                    //調味料を持つとき、見た目を引き継ぐ
                    if (_foodMeshRenderer.material.color == _foodTextureList.seasoningMaterial.color)
                    {
                        for (int i = 0; i < meshRenderers.Length; i++)
                        {
                            meshRenderers[i].material = _foodTextureList.seasoningMaterial;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        ///  食材の見た目変更で、参照するMeshRendererが変わる 卵の殻がmarterialを引き継ぐ Destroyより前に実行
        /// </summary>
        /// <param name="meshRenderers"></param>
        /// <param name="eggInsideRenderer"></param>
        protected void ChangeEggMeshRenderers(GameObject[] shells, MeshRenderer eggInsideRenderer)
        {
            if (_foodMeshRenderer.material.color == _foodTextureList.seasoningMaterial.color)
            {
                for (int i = 0; i < shells.Length; i++)
                {
                    var meshRenderer = shells[i].GetComponent<MeshRenderer>();
                    meshRenderer.material = _seasoningMaterial;
                }
                eggInsideRenderer.material = _foodTextureList.seasoningMaterial;
            }
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

