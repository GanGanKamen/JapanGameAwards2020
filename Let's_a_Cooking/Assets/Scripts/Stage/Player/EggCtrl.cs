using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class EggCtrl : MonoBehaviour
    {
        public int BreakCount { get { return breakCount; } }
        public bool HasBroken { get { return hasBroken; } }

        [SerializeField] private GameObject eggObject;
        [SerializeField] private Transform eggShells;
        [SerializeField] private GameObject eggInObject;
        [SerializeField] private Transform center;
        [SerializeField] private float breakPower;
        [SerializeField] private Material[] breakMaterials; //2種類 Length = 2

        private GameObject[] shells;
        private int breakCount = 0;
        private bool hasBroken = false;
        private void Awake()
        {
            var shellsList = new List<GameObject>();
            foreach(Transform child in eggShells)
            {
                shellsList.Add(child.gameObject);
            }
            shells = shellsList.ToArray();

        }

        public void EggCollide() //衝突してひびが入る
        {
            if (breakCount >= breakMaterials.Length) return;
            eggObject.GetComponent<MeshRenderer>().material = breakMaterials[breakCount];
            breakCount += 1;
        }

        public void EggBreak()
        {
            if (hasBroken) return;
            eggObject.SetActive(false);
            eggShells.gameObject.SetActive(true);
            for (int i = 0; i < shells.Length; i++)
            {
                var centerPos = center.position;
                var direction = shells[i].transform.position - centerPos;
                if (shells[i].GetComponent<Rigidbody>() == null) shells[i].AddComponent<Rigidbody>();
                var rb = shells[i].GetComponent<Rigidbody>();
                var force = breakPower * direction;
                rb.AddForce(force, ForceMode.VelocityChange);
            }
            if (eggInObject != null) eggInObject.SetActive(true);
            hasBroken = true;
            StartCoroutine(ShellsCoroutine());
        }

        private IEnumerator ShellsCoroutine()
        {
            yield return new WaitForSeconds(3f);
            Destroy(eggShells.gameObject);
            yield break;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}


