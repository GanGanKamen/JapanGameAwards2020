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
        /// マテリアルを指定したものに変更
        /// </summary>
        protected void ChangeMaterial(SkinnedMeshRenderer skinnedMeshRenderer , Material material)
        {
            skinnedMeshRenderer.material = material;
        }
    }

}

