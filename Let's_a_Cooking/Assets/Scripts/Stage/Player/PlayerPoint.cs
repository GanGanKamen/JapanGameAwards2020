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
            var pointInformation = _pointParameter.pointInformation[(int)pointType];
            //音と演出
            switch (pointInformation.pointOperator)
            {
                case PointOperator.Plus:
                    SoundManager.Instance.PlaySE(SoundEffectID.point_up);
                    EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_UP).parent = _foodStatus.FoodPositionNotRotate.transform;
                    break;
                case PointOperator.Minus:
                    SoundManager.Instance.PlaySE(SoundEffectID.point_down);
                    EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_Down).parent = _foodStatus.FoodPositionNotRotate.transform;
                    break;
                case PointOperator.Multiplication:
                    SoundManager.Instance.PlaySE(SoundEffectID.point_up);
                    EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_UP).parent = _foodStatus.FoodPositionNotRotate.transform;
                    break;
                case PointOperator.Division:
                    SoundManager.Instance.PlaySE(SoundEffectID.point_down);
                    EffectManager.Instance.InstantiateEffect(transform.position, EffectManager.EffectPrefabID.Point_Down).parent = _foodStatus.FoodPositionNotRotate.transform;
                    break;
                default:
                    break;
            }
            //ポイントの種類で演算方法が異なる
            //現状(2020/05/23)レア調味料のみ全ポイントに対して掛け算
            switch (pointType)
            {
                case GetPointType.EggBreaked:
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.EggCracked:
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.TouchSeasoning:
                    _canGetPointFlags[(int)GetPointOnTouch.Seasoning] = false;
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.TouchRareSeasoning:
                    _canGetPointFlags[(int)GetPointOnTouch.RareSeasoning] = false;
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.TouchBubble:
                    _canGetPointFlags[(int)GetPointOnTouch.Bubble] = false;
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.FirstWash:
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.FirstTowelTouch:
                    _isFirstTowel = false;
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.FallOffShrimpHead:
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.CutFood:
                    _canGetPointFlags[(int)GetPointOnTouch.Cut] = false;
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.GoalWithRareSeasoning:
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                case GetPointType.SeasoningWashAwayed:
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                //現状(2020/05/23)レア調味料のみ全ポイントに対して掛け算
                case GetPointType.RareSeasoningWashAwayed:
                    break;
                case GetPointType.TouchDirtDish:
                    _canGetPointFlags[(int)GetPointOnTouch.DirtDish] = false;
                    DefaultPointCalculate(pointInformation.pointOperator, pointInformation.pointValue);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// ポイント演算の基本形 各食材が現在持つものに対して計算 現状(2020/05/23)レア調味料のみ全ポイントに対して掛け算
        /// </summary>
        /// <param name="pointOperator">演算子</param>
        /// <param name="pointValue">獲得ポイント</param>
        private void DefaultPointCalculate(PointOperator pointOperator, int pointValue)
        {
            switch (pointOperator)
            {
                case PointOperator.Plus:
                    _getPoint += pointValue;
                    break;
                case PointOperator.Minus:
                    _getPoint -= pointValue;
                    //マイナスにはならない
                    if (_getPoint < 0)
                    {
                        _getPoint = 0;
                    }
                    break;
                //マイナスを考慮していない 例 -50 のとき -100点 _getPoint = 2 * Mathf.Abs(_getPoint);良くない方法
                case PointOperator.Multiplication:
                    _getPoint *= pointValue;
                    break;
                //マイナスを考慮 例 -50 のとき -50点 → -25点
                case PointOperator.Division:
                    _getPoint /= pointValue;
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
            _getPoint = 2 * Mathf.Abs(_getPoint);
            _firstPoint = 2 * Mathf.Abs(_getPoint);
            //自分が今までに獲得してきたすべてのポイントが倍になる StageSceneManagerに保存中
        }
    }
}
