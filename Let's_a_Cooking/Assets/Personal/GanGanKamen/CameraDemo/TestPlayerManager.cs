using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //UnityUIを使用

namespace GanGanKamen.Test
{
    public class TestPlayerManager : MonoBehaviour
    {
        [SerializeField] private Button topCamButton; //トップカメラに切り替えるボタン
        [SerializeField] private Button shootingCamButton; //シューティングカメラに切り替えるボタン
        [SerializeField] private Button shootButton; //仮の発射ボタン
        [SerializeField] private GameObject food; //食材
        [SerializeField] private TestCameraManager cameraManager; 
        private void Awake()
        {
            topCamButton.onClick.AddListener(() => TopCamButtonOnClick());  //ボタンに関数メソッドを登録
            shootingCamButton.onClick.AddListener(() => ShootCamButtonOnClick());
            shootButton.onClick.AddListener(() => Shoot());
        }

        private void TopCamButtonOnClick()
        {
            cameraManager.ChangeToTop();
        }

        private void ShootCamButtonOnClick()
        {
            cameraManager.ChangeToShoot();
        }

        private void Shoot()
        {
            var rb = food.GetComponent<Rigidbody>();
            rb.AddForce(new Vector3(0, 10f, 10f), ForceMode.VelocityChange);
        }
    }
}


