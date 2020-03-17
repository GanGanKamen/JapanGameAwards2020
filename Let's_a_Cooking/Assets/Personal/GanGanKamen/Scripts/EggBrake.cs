using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GanGanKamen
{
    public class EggBrake : MonoBehaviour
    {
        [SerializeField] private Transform center;
        [SerializeField] private float power;
        private Vector3 centerPosition;
        private bool isBraked = false;

        private void Awake()
        {
            centerPosition = center.position;
        }

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.B) && isBraked == false)
            {
                BrakeUp();
            }
        }

        private void BrakeUp()
        {
            for(int i = 0;i < transform.childCount; i++)
            {
                var debris = transform.GetChild(i);
                var force = (debris.position - centerPosition).normalized;
                debris.gameObject.AddComponent<Rigidbody>();
                debris.GetComponent<Rigidbody>().AddForce(force * power, ForceMode.VelocityChange);
            } 
        }
    }

}
