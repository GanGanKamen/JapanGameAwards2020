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
		// Start is called before the first frame update
		void Start()
        {
            
        }


        // Update is called once per frame
        void Update()
		{
        }
        /// <summary>
        /// 調味料を得る
        /// </summary>
        protected void GetSeasoning(SkinnedMeshRenderer skinnedMeshRenderer , Material material)
        {
            //動かないので調整中
            //skinnedMeshRenderer.materials[0] = material;
            skinnedMeshRenderer.materials[0].color = Color.black;
        }
        /// <summary>
        /// 水で洗う
        /// </summary>
        /// <param name="skinnedMeshRenderer"></param>
        /// <param name="material"></param>
        protected void CleanSeasoning(SkinnedMeshRenderer skinnedMeshRenderer, Material material)
        {
            skinnedMeshRenderer.materials[0].color = Color.white;
        }
    }

}

