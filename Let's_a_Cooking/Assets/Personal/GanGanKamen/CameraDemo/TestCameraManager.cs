using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;  //cinemachineを使用

namespace GanGanKamen.Test
{
    public class TestCameraManager : MonoBehaviour
    {
        [SerializeField] CinemachineVirtualCamera shootingCam = null; //シューティングカメラを参照
        [SerializeField] CinemachineVirtualCamera topCam = null;　//トップカメラを参照
        
        public void ChangeToShoot()
        {
            shootingCam.Priority = 10;
            topCam.Priority = 0;
        }

        public void ChangeToTop()
        {
            shootingCam.Priority = 0;
            topCam.Priority = 10;
        }
    }

}

