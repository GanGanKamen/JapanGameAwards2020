using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class Egg : MonoBehaviour
    {
        /// <summary>
        /// 割れた回数 ショットによるキッチンとの衝突で増加
        /// </summary>
        public int BreakCount { get { return breakCount; } }
        /// <summary>
        /// 割れているかどうか
        /// </summary>
        public bool HasBroken { get { return hasBroken; } }

        [SerializeField] private GameObject eggObject = null;
        [SerializeField] private Transform eggShells = null;
        [SerializeField] private GameObject eggInObject = null;
        [SerializeField] private Transform center = null;
        [SerializeField] private float breakPower = 0;
        public Material[] BreakMaterials
        {
            get { return breakMaterials; }
        }
        [SerializeField] private Material[] breakMaterials = null; //2種類 Length = 2
        /// <summary>
        /// 割れた後のMeshRenderer 
        /// </summary>
        public MeshRenderer InsideMeshRenderer
        {
            get { return _insideMeshRenderer; }
        }
        [SerializeField] private MeshRenderer _insideMeshRenderer = null;

        public GameObject[] Shells
        {
            get { return shells; }
        }
        private GameObject[] shells;
        private int breakCount = 0;
        private bool hasBroken = false;
        public bool IsFirstCollision
        {
            get { return _isFirstCollsion; }
        }
        /// <summary>
        /// ショットによる一回目の衝突のみ割れる スタート地点に帰ってきたとき少し落下する事に注意 
        /// </summary>
        private bool _isFirstCollsion = false;
        /// <summary>
        /// ショット開始時呼ばれる 割れるようになる
        /// </summary>
        public void FlagReset()
        {
            _isFirstCollsion = true;
        }

        private void Awake()
        {
            var shellsList = new List<GameObject>();
            foreach (Transform child in eggShells)
            {
                shellsList.Add(child.gameObject);
            }
            shells = shellsList.ToArray();

        }
        /// <summary>
        /// 衝突してひびが入る
        /// </summary>
        public void EggCollide(bool haveSeasoning)
        {
            if (breakCount >= breakMaterials.Length) return;
            if (!haveSeasoning)
                eggObject.GetComponent<MeshRenderer>().material = breakMaterials[breakCount];
            breakCount += 1;
            _isFirstCollsion = false;
        }
        /// <summary>
        /// 卵が割れる
        /// </summary>
        public void EggBreak()
        {
            if (hasBroken) return;
            eggObject.SetActive(false);
            eggShells.gameObject.SetActive(true);
            eggShells.transform.parent = null;
            for (int i = 0; i < shells.Length; i++)
            {
                var centerPos = center.position;
                var direction = shells[i].transform.position - centerPos;
                if (shells[i].GetComponent<Rigidbody>() == null) shells[i].AddComponent<Rigidbody>();
                var rb = shells[i].GetComponent<Rigidbody>();
                var force = breakPower * direction;
                rb.AddForce(force, ForceMode.VelocityChange);
                rb.mass /= 10;
            }
            if (eggInObject != null) eggInObject.SetActive(true);
            hasBroken = true;
            //StartCoroutine(ShellsCoroutine());
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


