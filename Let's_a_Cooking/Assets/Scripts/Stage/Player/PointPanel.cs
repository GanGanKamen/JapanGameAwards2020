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

        private IEnumerator pointChangeCoroutine;
        private Transform target;
        private float hight;
        private PlayerPoint player;
        private int pointChangeMode = 0;
        private float pointChangeDelta;
        private float animationPt = 0;
        private float goalPt = 0;

        private void Start()
        {
            //PointChange(300, 900);
        }

        private void Update()
        {
            PointChangeAnimation();
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
            pointChangeCoroutine = PointChangeCoroutine(prePoint, nowPoint);
            StartCoroutine(pointChangeCoroutine);
        }

        public void Cancellation()
        {
            if(pointChangeCoroutine != null)
            {
                StopCoroutine(pointChangeCoroutine);
                pointChangeCoroutine = null;
            }
            Destroy(gameObject);
        }

        private void SetPosition()
        {
            if (target == null) return;
            transform.position = target.position + new Vector3(0, hight, 0);
        }


        private IEnumerator PointChangeCoroutine(int prePoint,int nowPoint)
        {
            Init();
            var pointDifference = nowPoint - prePoint;
            if (pointDifference == 0 || isPlayPointChange) yield break;
            if(player != null) player.nowPointPanel = this;
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
            pointChangeDelta = Mathf.Abs(pointDifference) * Time.deltaTime / pointChangeTime;
            animationPt = prePoint;
            goalPt = nowPoint;
            if (pointDifference > 0)
            {
                pointChangeMode = 1;   
            }
            else if(pointDifference < 0)
            {
                pointChangeMode = 2;
            }

            while(pointChangeMode != 0)
            {
                yield return null;
            }

            pointText.text = nowPoint.ToString();
            yield return new WaitForSeconds(newPointTime);
            Init();
            isPlayPointChange = false;
            if (player != null) player.nowPointPanel = null;
            Destroy(gameObject);
            yield break;
        }

        private void PointChangeAnimation()
        {
            switch (pointChangeMode)
            {
                case 0:
                    break;
                case 1:
                    animationPt = animationPt + pointChangeDelta;
                    pointText.text = ((int)animationPt).ToString();
                    if (animationPt >= goalPt) pointChangeMode = 0;
                    break;
                case 2:
                    animationPt = animationPt - pointChangeDelta;
                    pointText.text = ((int)animationPt).ToString();
                    if (animationPt <= goalPt) pointChangeMode = 0;
                    break;
            }
        }
    }
}


