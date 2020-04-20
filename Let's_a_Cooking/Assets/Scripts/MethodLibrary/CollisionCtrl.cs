using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GanGanKamen {
    public class CollisionCtrl : MonoBehaviour
    {
        [SerializeField]private GameObject colliderObj;

        public void SetAngleX(float angle)
        {
            var preAngle = colliderObj.transform.localEulerAngles;
            colliderObj.transform.localEulerAngles = new Vector3(angle, preAngle.y, preAngle.z);
        }

        public void SetPositionY(float posY)
        {
            var prepos= colliderObj.transform.localPosition;
            colliderObj.transform.localPosition = new Vector3(prepos.x, posY, prepos.z);
        }
    }
}


