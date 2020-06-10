using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    public class PointPanel : MonoBehaviour
    {
        
        [SerializeField] private float pointDifferenceTime;
        [SerializeField] private float prePointTime;
        [SerializeField] private float pointChangeTime;
        [SerializeField] private float newPointTime;
        [SerializeField] private GameObject panelObject;
        [SerializeField] private TMPro.TextMeshPro pointText;

        public bool IsPlayPointChange { get { return isPlayPointChange; } }

        private bool isPlayPointChange = false;

        private Transform target;
        private float hight;
        private PlayerPoint player;

        private void FixedUpdate()
        {
            SetPosition();
        }

        public void Init(Transform _target, float _hight,PlayerPoint _player)
        {
            panelObject.SetActive(false);
            pointText.text = "";
            target = _target;
            hight = _hight;
            player = _player;
        }


        public void Init()
        {
            panelObject.SetActive(false);
            pointText.text = "";
        }

        public void PointChange(int prePoint, int nowPoint)
        {
            StartCoroutine(PointChangeCoroutine(prePoint, nowPoint));
        }

        private void SetPosition()
        {
            transform.position = target.position + new Vector3(0, hight, 0);
        }


        private IEnumerator PointChangeCoroutine(int prePoint,int nowPoint)
        {
            Init();
            var pointDifference = nowPoint - prePoint;
            if (pointDifference == 0 || isPlayPointChange) yield break;
            player.isPlayPointPanel = true;
            isPlayPointChange = true;
            var differenceText = "";
            if (pointDifference > 0) differenceText = "+" + pointDifference.ToString();
            else differenceText = pointDifference.ToString();
            panelObject.SetActive(true);
            pointText.text = differenceText;
            if (pointDifferenceTime <= 0) pointDifferenceTime = 1;
            yield return new WaitForSeconds(pointDifferenceTime);
            pointText.text = prePoint.ToString();
            yield return new WaitForSeconds(prePointTime);
            if (pointChangeTime <= 0) pointChangeTime = 1;
            var pointChangeDelta = pointDifference * Time.deltaTime / pointChangeTime;
            float pt = prePoint;
            if (pointDifference > 0)
            {

                do
                {
                    pt = pt + pointChangeDelta;
                    pointText.text = ((int)pt).ToString();
                    yield return null;
                } while (pt < nowPoint);
            }
            else
            {
                do
                {
                    pt = pt - pointChangeDelta;
                    pointText.text = ((int)pt).ToString();
                } while (pt > nowPoint);
            }
            pointText.text = nowPoint.ToString();
            yield return new WaitForSeconds(newPointTime);
            Init();
            isPlayPointChange = false;
            player.isPlayPointPanel = false;
            Destroy(gameObject);
            yield break;
        }

    }
}


