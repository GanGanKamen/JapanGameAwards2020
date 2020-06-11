using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class Egg : MonoBehaviour
    {
        public CapsuleCollider eggCollider = null;
        /// <summary>
        ///卵が割れた後のコライダー
        /// </summary>
        public BoxCollider insideBoxCollider = null;
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
        /// <summary>
        /// ひびの入ったマテリアル
        /// </summary>
        public Material[] BreakMaterials
        {
            get { return breakMaterials; }
        }
        /// <summary>
        /// ひびの入ったマテリアル
        /// </summary>
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
            SoundManager.Instance.PlaySE(SoundEffectID.egg_break);
            SoundManager.Instance.PlaySE(SoundEffectID.egg_app);
            var isGroundedArea = GetComponent<FoodStatus>()?.IsGroundedArea;
            isGroundedArea.transform.localPosition = new Vector3(isGroundedArea.transform.localPosition.x, -0.0064f, -isGroundedArea.transform.localPosition.z);
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
            var food = GetComponent<FoodStatus>();

            //if (food.IsSeasoningMaterial)
            //{
            //    food.ChangeMaterial(StageSceneManager.Instance.FoodTextureList.seasoningMaterial, food.FoodType, food.OriginalFoodProperty);
            //}

        }
    }
}


