using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class Shot : MonoBehaviour
    {
        new Rigidbody rigidbody;
        // Start is called before the first frame update
        void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                rigidbody.AddForce(new Vector3(0, 1, 1) * 4);
            }
        }
    }
}
