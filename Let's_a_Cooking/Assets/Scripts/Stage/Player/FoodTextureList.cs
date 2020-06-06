using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public enum CutDifferentTextureFood
    {
        Chicken,Sausage
    }
    public class FoodTextureList : MonoBehaviour
    {
        /// <summary>Shrimpのしっぽ 0 ,Eggの中身 1,Chicken 2,Sausage 3 Shrimpの頭 4</summary>
        [Header("Shrimpのしっぽ 0 ,Eggの中身 1,Chicken 2,Sausage 3 , Shrimpの頭 4")] public Texture[] normalTextures = new Texture[System.Enum.GetValues(typeof(FoodType)).Length + 1];
        /// <summary>Shrimpのしっぽ 0 ,Eggの中身 1,Chicken 2,Sausage 3 Shrimpの頭 4</summary>
        [Header("Shrimpのしっぽ 0 ,Eggの中身 1,Chicken 2,Sausage 3 , Shrimpの頭 4")] public Texture[] seasoningFoodTextures = new Texture[System.Enum.GetValues(typeof(FoodType)).Length + 1];
        /// <summary>Chicken 0,Sausage 1</summary>
        [Header("Chicken 0,Sausage 1")] public Texture[] cutFoodTextures = new Texture[System.Enum.GetValues(typeof(CutDifferentTextureFood)).Length];
        /// <summary>Chicken 0,Sausage 1</summary>
        [Header("Chicken 0,Sausage 1")] public Texture[] cutSeasoningFoodTextures = new Texture[System.Enum.GetValues(typeof(CutDifferentTextureFood)).Length];
        /// <summary>Shrimpのしっぽ 0 ,Eggの中身 1,Chicken 2,Sausage 3 Shrimpの頭 4</summary>
        [Header("Shrimpのしっぽ 0 ,Eggの中身 1,Chicken 2,Sausage 3 , Shrimpの頭 4")] public Material[] normalFoodMaterials = new Material[System.Enum.GetValues(typeof(FoodType)).Length + 1];
        /// <summary>Shrimpのしっぽ 0 ,Eggの中身 1,Chicken 2,Sausage 3 Shrimpの頭 4</summary>
        [Header("Shrimpのしっぽ 0 ,Eggの中身 1,Chicken 2,Sausage 3 , Shrimpの頭 4")]
        public Material[] seasoningFoodMaterials = new Material[System.Enum.GetValues(typeof(FoodType)).Length + 1];
        /// <summary>Chicken 0,Sausage 1</summary>
        [Header("Chicken 0,Sausage 1")]public Material[] cutFoodMaterials = new Material[System.Enum.GetValues(typeof(CutDifferentTextureFood)).Length];
        /// <summary>Chicken 0,Sausage 1</summary>
        [Header("Chicken 0,Sausage 1")]public Material[] cutSeasoningFoodMaterials = new Material[System.Enum.GetValues(typeof(CutDifferentTextureFood)).Length];
        [Header("調味料の色は共通でこのmaterialの色")] public Material seasoningMaterial;
    }
}
