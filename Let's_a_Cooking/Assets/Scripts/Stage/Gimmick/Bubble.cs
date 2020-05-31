using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
    public class Bubble : MonoBehaviour
    {
        public Rigidbody BubbleRigidbody
        {
            get { return _bubbleRigidbody; }
        }
        /// <summary>
        /// ランダムウォーク用
        /// </summary>
        private Rigidbody _bubbleRigidbody;
        private Vector3 _speedVectorOfBubble;
        [SerializeField] private float _maxSpeedOfBubble = 0.4f;
        private float _changeDirectionTimeCounterOfBubble;
        /// <summary>
        /// あわの進行方向を変えるまでにかかる時間(乱数取得)
        /// </summary>
        private float _changeDirectionTimeOfBubble;
        /// <summary>
        ///あわ移動許容範囲の2端(x.y.zの最小値と最大値)の座標 この2点の間の座標に発生させる
        /// </summary>
        private Vector3[] _bubbleLimitPosition;
        // Start is called before the first frame update
        void Start()
        {
            _bubbleRigidbody = GetComponent<Rigidbody>();
            ChangeBubbleSpeedVectorDirection();
        }

        // Update is called once per frame
        void Update()
        {
            if (_changeDirectionTimeCounterOfBubble >= _changeDirectionTimeOfBubble)
            {
                ChangeBubbleSpeedVectorDirection();
            }
            else
            {
                _changeDirectionTimeCounterOfBubble += Time.deltaTime;
            }
            if (_bubbleLimitPosition != null)
            {
                if (transform.position.x < _bubbleLimitPosition[(int)LimitValue.Min].x || transform.position.y < _bubbleLimitPosition[(int)LimitValue.Min].y || transform.position.z < _bubbleLimitPosition[(int)LimitValue.Min].z)
                {
                    ReverseBubbleSpeedVectorDirection();
                }
                else if (transform.position.x > _bubbleLimitPosition[(int)LimitValue.Max].x || transform.position.y > _bubbleLimitPosition[(int)LimitValue.Max].y || transform.position.z > _bubbleLimitPosition[(int)LimitValue.Max].z)
                {
                    ReverseBubbleSpeedVectorDirection();
                }
                transform.position     
                = new Vector3(Mathf.Clamp(transform.position.x, _bubbleLimitPosition[(int)LimitValue.Min].x, _bubbleLimitPosition[(int)LimitValue.Max].x),
                Mathf.Clamp(transform.position.y, _bubbleLimitPosition[(int)LimitValue.Min].y, _bubbleLimitPosition[(int)LimitValue.Max].y),
                Mathf.Clamp(transform.position.z, _bubbleLimitPosition[(int)LimitValue.Min].z, _bubbleLimitPosition[(int)LimitValue.Max].z));
            }
            else
            {
                Debug.Log("あわが限界エリアを検知できませんでした。");
            }
            _bubbleRigidbody.velocity = _speedVectorOfBubble;
        }
        public void ChangeBubbleSpeedVectorDirection()
        {
            _speedVectorOfBubble = new Vector3(Cooking.Random.GetRandomFloat(-_maxSpeedOfBubble, _maxSpeedOfBubble), Cooking.Random.GetRandomFloat(-_maxSpeedOfBubble, _maxSpeedOfBubble), Cooking.Random.GetRandomFloat(-_maxSpeedOfBubble, _maxSpeedOfBubble));
            _changeDirectionTimeOfBubble = Cooking.Random.GetRandomFloat(1, 5);
            _changeDirectionTimeCounterOfBubble = 0;
        }
        /// <summary>
        /// 限界エリアに来た時に、速度ベクトルを反転させる
        /// </summary>
        public void ReverseBubbleSpeedVectorDirection()
        {
            _speedVectorOfBubble = -_speedVectorOfBubble;
            _changeDirectionTimeOfBubble = Cooking.Random.GetRandomFloat(1, 5);
            _changeDirectionTimeCounterOfBubble = 0;
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == TagList.BubbleZone.ToString())
            {
                var referencePoint = other.transform.GetChild(0).position;
                _bubbleLimitPosition = new Vector3[] { referencePoint, referencePoint + other.transform.localScale };
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == TagList.BubbleZone.ToString())
            {
                ReverseBubbleSpeedVectorDirection();
            }
        }
    }
}
