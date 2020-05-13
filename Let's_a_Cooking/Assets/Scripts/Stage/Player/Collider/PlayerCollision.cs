using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 親子関係やRigidbodyとの位置関係次第では必要になる 現状不要(04/16)
    /// </summary>
    public class PlayerCollision : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == TagList.Floor.ToString())
            {
                //_isFall = true;
            }
            //初期化時以外でも使用するなら、範囲指定などの必要がある
            //else if (collision.gameObject.tag == "Kitchen") 現状タグ無し
            {
                //_onKitchen = true;
            }
            //Debug.Log("衝突");
        }
        private void OnTriggerEnter(Collider other)
        {
            //if (other.tag == TagList.Finish.ToString())
            //{
            //    _isGoal = true;
            //}
            //if (other.tag == TagList.Water.ToString())
            //{
            //    CleanSeasoning(_skinnedMeshRenderer, _ebi);
            //}
            ///// とりあえず調味料はトリガーで
            //else if (other.tag == TagList.Seasoning.ToString())
            //{
            //    GetSeasoning(_skinnedMeshRenderer, _ebiBlack);
            //    Destroy(other.gameObject);
            //    Debug.Log(_skinnedMeshRenderer.materials[0]);
            //}
        }
    }

}
