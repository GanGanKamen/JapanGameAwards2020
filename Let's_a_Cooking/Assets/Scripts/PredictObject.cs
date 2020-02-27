using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class PredictObject : MonoBehaviour
    {
        //予測オブジェクトが消えるまでのカウンター
        float desteoyCounter = 0;
                /// <summary>
        /// 予測線表示時間間隔です。
        /// </summary>
        public float predictObjInterval;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //予測オブジェクトが消えるまでのカウンターを加算する
            //desteoyCounter += 1 * Time.deltaTime;

            //4/10秒経ったとき
            if(desteoyCounter >= 0.4f)
            {
                //その予測オブジェクトの消去
                //Destroy(this.gameObject);
            }
        }
    }

}