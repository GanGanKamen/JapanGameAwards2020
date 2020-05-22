using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Cooking.Stage
{
    /// <summary>
    /// bool配列 CanGetPoint用
    /// </summary>
    public enum GetPointOnTouch
    {
        DirtDish,//減点
        Bubble,
        Seasoning,
        RareSeasoning,
        Cut
    }
    /// <summary>
    /// アクティブプレイヤーのプレイヤーポイント取得のため、ターンコントローラーを経由してポイント取得
    /// </summary>
    public class PlayerPoint : MonoBehaviour
    {
        /// <summary>
        /// 初期ポイント + 獲得ポイント
        /// </summary>
        public int Point
        {
            get { return _firstPoint + _getPoint; }
        }
        /// <summary>
        /// 初期ポイント
        /// </summary>
        private int _firstPoint = 100;
        /// <summary>
        /// 獲得ポイント
        /// </summary>
        private int _getPoint = 0;
        public bool IsFirstTowel
        {
            get { return _isFirstTowel; }
        }
        public bool IsFirstWash
        {
            get { return _isFirstWash; }
        }
        /// <summary>
        /// 初回フラグ
        /// </summary>
        private bool _isFirstWash = true, _isFirstTowel = true;
        /// <summary>
        /// ポイント取得可能フラグ enum GetPointOnTouch
        /// </summary>
        public bool[] CanGetPointFlags
        {
            get { return _canGetPointFlags; }
        }
        /// <summary>
        /// ポイント取得可能フラグ
        /// </summary>
        bool[] _canGetPointFlags = new bool[Enum.GetValues(typeof(GetPointOnTouch)).Length];
        /// <summary>
        /// 自分自身のステータス レア調味料を持つかどうかはStageSceneが判定するからいらない予定
        /// </summary>
        private FoodStatus _foodStatus;
        PointParameter _pointParameter = null;

        private void Awake()
        {
            _pointParameter = Resources.Load<PointParameter>("ScriptableObjects/PointParameter");
        }

        /// <summary>
        /// ショット時に呼ばれる アニメーションなどで触れるのでこのタイミング 衝突時ポイント獲得フラグのリセット
        /// </summary>
        public void ResetGetPointBool()
        {
            for (int i = 0; i < _canGetPointFlags.Length; i++)
            {
                _canGetPointFlags[i] = true;
            }
        }

        private void OnEnable()
        {
            _foodStatus = GetComponent<FoodStatus>();
            ComponentCheck.CheckNecessaryCopmonent<FoodStatus>(this, true);
        }
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// ポイントが増減する際に呼ばれる
        /// </summary>
        /// <param name="pointType"></param>
        public void GetPoint(GetPointType pointType)
        {
            switch (_pointParameter.pointInformation[(int)pointType].pointOperator)
            {
                case PointOperator.Plus:
                    EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_UP).parent = _foodStatus.FoodPositionNotRotate.transform;
                    break;
                case PointOperator.Minus:
                    EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_Down).parent = _foodStatus.FoodPositionNotRotate.transform;
                    break;
                case PointOperator.Multiplication:
                    EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_UP).parent = _foodStatus.FoodPositionNotRotate.transform;
                    break;
                case PointOperator.Division:
                    EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_Down).parent = _foodStatus.FoodPositionNotRotate.transform;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// レア調味料を持ってゴール
        /// </summary>
        private void GoalWithRareSeasoning()
        {
            //マイナスを考慮 例 -50 のとき 50点 → 100点
            _getPoint = 2 * Mathf.Abs(_getPoint);
            _firstPoint = 2 * Mathf.Abs(_getPoint);
        }
        /// <summary>
        /// 普通の調味料が洗い流される
        /// </summary>
        private void SeasoningWashAwayed()
        {
            _getPoint /= 2;
        }
        /// <summary>
        /// 調味料レアが洗い流される
        /// </summary>
        private void RareSeasoningWashAwayed()
        {
            _getPoint /= 2;
        }
        /// <summary>
        /// 卵に亀裂が入る
        /// </summary>
        private void EggCracked()
        {
            _getPoint += 10;
        }
        /// <summary>
        /// 食材が切られたときに呼ばれる 卵は割れたとき エビは別のメソッド FallOffShrimpHead
        /// </summary>
        private void CutFood()
        {
            EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_UP).parent = _foodStatus.FoodPositionNotRotate.transform;
            SoundManager.Instance.PlaySE(SoundEffectID.point_up);
            _getPoint += 200;
        }
        /// <summary>
        /// あわに触れる
        /// </summary>
        private void TouchBubble()
        {
            _getPoint += 10;
            _canGetPointFlags[(int)GetPointOnTouch.Bubble] = false;
        }
        /// <summary>
        /// 調味料に触れる
        /// </summary>
        private void TouchSeasoning()
        {
            EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_UP).parent = _foodStatus.FoodPositionNotRotate.transform;
            SoundManager.Instance.PlaySE(SoundEffectID.point_up);
            //マイナスを考慮 例 -50 のとき 50点 → 100点
            _getPoint = 2 * Mathf.Abs(_getPoint);
            _canGetPointFlags[(int)GetPointOnTouch.Seasoning] = false;
        }
        /// <summary>
        /// レア調味料に触れる
        /// </summary>
        private void TouchRareSeasoning()
        {
            //マイナスを考慮 例 -50 のとき 50点 → 100点
            _getPoint = 2 * Mathf.Abs(_getPoint);
            _canGetPointFlags[(int)GetPointOnTouch.RareSeasoning] = false;
        }
        /// <summary>
        /// 汚れた皿に触れる
        /// </summary>
        private void TouchDirtDish()
        {
            EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_Down).parent = _foodStatus.FoodPositionNotRotate.transform;
            SoundManager.Instance.PlaySE(SoundEffectID.point_down);
            _getPoint -= 50;
            if (_firstPoint + _getPoint < 0)
            {
                _getPoint = -100;
            }
            _canGetPointFlags[(int)GetPointOnTouch.DirtDish] = false;
        }
        /// <summary>
        /// 壁に触れる 頭が取れていないことが条件
        /// </summary>
        private void TouchWall()
        {
            EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_UP).parent = _foodStatus.FoodPositionNotRotate.transform;
            SoundManager.Instance.PlaySE(SoundEffectID.point_up);
            _getPoint += 200;
        }
        /// <summary>
        /// 初めて水洗い
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private void FirstWash()
        {
            _getPoint += 50;
            _isFirstWash = false;
        }
        /// <summary>
        /// 初めてタオルに触れる
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private void FirstTowelTouch()
        {
            EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_UP).parent = _foodStatus.FoodPositionNotRotate.transform;
            SoundManager.Instance.PlaySE(SoundEffectID.point_up);
            _getPoint += 50;
            _isFirstTowel = false;
        }
    }
}
