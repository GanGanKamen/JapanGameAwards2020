using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Test
{
    public class FoodBound : MonoBehaviour
    {
        new Rigidbody rigidbody = null;
        // Start is called before the first frame update
        void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y, 2);
        }
    }

}