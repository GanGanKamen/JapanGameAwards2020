using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    public class ParabolaRayCast : MonoBehaviour
    {
        Vector3 initialSpeedVector;
        float flyTime = 0;
        float height = 0.1f;

        bool start;
        // Start is called before the first frame update
        void Start()
        {
            initialSpeedVector = new Vector3(1, 1f, 1) * 10;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
            {
                //無限ループ防止目的 滞空時間100秒制限
                //for (float flyTime = 0; flyTime < 100; flyTime += 0.001f)
                //{
                //    this.height = initialSpeedVector.y * flyTime - 0.5f * 9.81f * flyTime * flyTime;
                //    //if (CastRay(flyTime, this.height))
                //        break;
                //}
            }
        }

        private bool CastRay(Vector3 speedVector, float time, float height )
        {
            ///レイの長さ
            float length = 1f;
            Vector3 origin = transform.position + new Vector3(speedVector.x * time, height, speedVector.z * time);
            Vector3 direction = origin + new Vector3(speedVector.x, speedVector.y - 9.81f * time, speedVector.z) * 0.1f;
            //Rayが当たったオブジェクトの情報を入れる箱
            RaycastHit hit; //原点 方向
            Ray ray = new Ray(origin, direction);
            Physics.Raycast(ray, out hit, length);
            Debug.DrawLine(origin, direction * length, Color.red, 30);
            if (Physics.Raycast(ray, out hit, length))
            {
                Debug.Log(hit.point);
                return true;
            }
            return false;
        }
    }
}
