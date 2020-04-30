using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GanGanKamen.Test
{
    public class FoodLookPoint : MonoBehaviour
    {
        [SerializeField] private GameObject food = null;

        private void FixedUpdate() //RigidbodyのVelocity変化を追跡するので、リアルタイムベースのFixedUpdate()を使用する
        {
            transform.position = food.transform.position + new Vector3(0,1,0);
        }
    }
}


