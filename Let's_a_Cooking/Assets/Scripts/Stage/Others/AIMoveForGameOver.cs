using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class AIMoveForGameOver : MonoBehaviour
    {
        Transform referencePoint;
        private void Start()
        {
            referencePoint = transform.GetChild(0);
        }
        public Transform Move()
        {
            return referencePoint;
        }
    }
}
