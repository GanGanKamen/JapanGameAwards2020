using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 予測線を生成
    /// 回転に合わせて飛ばす先の地点を計算して、予測線を飛ばす地点を変更する。
    /// </summary>
    public class PredictLineManager : MonoBehaviour
    {
        /// <summary>
        /// 予測線表示用のプレハブオブジェクト。
        /// </summary>
        [SerializeField] GameObject _predictObjectPrefab = null;
        /// <summary>
        /// 現在シーン上に存在している予測線オブジェクトが入る。
        /// </summary>
        [SerializeField]private List<PredictLine> _predictLines = new List<PredictLine>();
        /// <summary>
        /// 予測落下地点表示用プレハブオブジェクト。
        /// </summary>
        [SerializeField] GameObject _predictShotPointPrefab = null;
        /// <summary>
        /// 現在シーン上に存在している予測落下地点オブジェクトが入る。
        /// </summary>
        private GameObject _predictShotPoint = null;
        /// <summary>
        /// 予測線が出ている時間カウント。
        /// </summary>
        private float _predictTimeCounter;
        /// <summary>
        /// 予測線が出てくる時間間隔。
        /// </summary>
        [SerializeField] float _predictTimeInterval = 0.1f;
        /// <summary>
        /// 予測線が消えるまでの時間。タイムカウンターは各predictlineが持つ。
        /// </summary>
        [SerializeField] float _destroyTime = 1f;
        /// <summary>
        /// 表示される予測線の数。予測線を表示する時間と、表示時間間隔によって決まる。
        /// </summary>
        private int _displayCountOfpredictLines = 10;
        /// <summary>
        /// 削除するオブジェクトの番号。古いものから順に削除。
        /// </summary>
        private int _destroyIndex = 0;
        private Vector3 _instantiatePosition;
        /// <summary>
        /// ヒエラルキー整頓用の親オブジェクト
        /// </summary>
        [SerializeField] GameObject _predictLinesParent = null;

        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得。
        /// </summary>
        public static PredictLineManager Instance
        {
            get { return _instance; }
        }
        static PredictLineManager _instance = null;

        /// <summary>
        /// Start()より先に実行
        /// </summary>
        private void Awake()
        {
            _instance = this;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            _predictShotPoint = Instantiate(_predictShotPointPrefab);
            //割り切れる数字である想定。
            _displayCountOfpredictLines = Mathf.RoundToInt(_destroyTime / _predictTimeInterval);
        }
        // Update is called once per frame
        void Update()
        {
            if (!TurnManager.Instance.IsAITurn )
            {
                var shotManager = ShotManager.Instance;
                var maxShotSpeedVector = shotManager.CalculateMaxShotPowerVector();
                //予測線を管理。
                switch (UIManager.Instance.MainUIStateProperty)
                {
                    case ScreenState.InitializeChoose:
                        break;
                    case ScreenState.Start:
                        PredictLineManage(maxShotSpeedVector);
                        break;
                    case ScreenState.FrontMode:
                        //予測線を飛ばす方向を取得。
                        PredictLineManage(maxShotSpeedVector);
                        break;
                    case ScreenState.SideMode:
                        //予測線を飛ばす方向を取得。
                        PredictLineManage(maxShotSpeedVector);
                        break;
                    case ScreenState.LookDownMode:
                        //予測線を飛ばす方向を取得。
                        PredictLineManage(maxShotSpeedVector);
                        break;
                    case ScreenState.ShottingMode:
                        break;
                    case ScreenState.Finish:
                        break;
                    case ScreenState.Pause:
                        break;
                    default:
                        break;
                }
                ///予測線の挙動
                switch (UIManager.Instance.MainUIStateProperty)
                {
                    case ScreenState.InitializeChoose:
                        break;
                    case ScreenState.Start:
                        //落下地点の変更を予測線にも伝える。
                        //ChangePredictFallPointOnXZ(maxShotSpeedVector);
                        //PredictPhysicsManage();
                        break;
                    case ScreenState.FrontMode:
                        {
                            var activeFood = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex];
                            //落下地点の変更を予測線にも伝える。
                            ChangePredictFallPointOnXZ(maxShotSpeedVector);
                            //食材の種類別に落下予測地点座標を取得
                            GetPredictFallPointByRayCast(maxShotSpeedVector, activeFood);
                            PredictPhysicsManage();
                        }
                        break;
                    case ScreenState.SideMode:
                        {
                            var activeFood = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex];
                            //落下地点の変更を予測線にも伝える。
                            ChangePredictFallPointOnXZ(maxShotSpeedVector);
                            //食材の種類別に落下予測地点座標を取得
                            GetPredictFallPointByRayCast(maxShotSpeedVector, activeFood);
                            PredictPhysicsManage();
                        }
                        break;
                    case ScreenState.LookDownMode:
                        {
                            var activeFood = TurnManager.Instance.FoodStatuses[TurnManager.Instance.ActivePlayerIndex];
                            //落下地点の変更を予測線にも伝える。
                            ChangePredictFallPointOnXZ(maxShotSpeedVector);
                            //食材の種類別に落下予測地点座標を取得
                            GetPredictFallPointByRayCast(maxShotSpeedVector, activeFood);
                            PredictPhysicsManage();
                        }
                        break;
                    case ScreenState.ShottingMode:
                        break;
                    case ScreenState.Finish:
                        break;
                    case ScreenState.Pause:
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 食材の種類別に落下予測地点座標を取得
        /// </summary>
        /// <param name="maxShotSpeedVector"></param>
        /// <param name="activeFood"></param>
        private void GetPredictFallPointByRayCast(Vector3 maxShotSpeedVector, FoodStatus activeFood)
        {
            switch (activeFood.FoodType)
            {
                case FoodType.Shrimp:
                    _predictShotPoint.transform.position = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector3>(transform.position, maxShotSpeedVector, ShotManager.Instance.GetShotAngleX(), activeFood.FoodType, activeFood.GetColliderSize<Vector3>());
                    break;
                case FoodType.Egg:
                    if (activeFood.OriginalFoodProperty.egg.HasBroken)
                    {
                        _predictShotPoint.transform.position = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector3>(transform.position, maxShotSpeedVector, ShotManager.Instance.GetShotAngleX(), activeFood.FoodType, activeFood.GetColliderSize<Vector3>());
                    }
                    else
                    {
                        _predictShotPoint.transform.position = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector2>(transform.position, maxShotSpeedVector, ShotManager.Instance.GetShotAngleX(), activeFood.FoodType, activeFood.GetColliderSize<Vector2>());
                    }
                    break;
                case FoodType.Chicken:
                    _predictShotPoint.transform.position = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector3>(transform.position, maxShotSpeedVector, ShotManager.Instance.GetShotAngleX(), activeFood.FoodType, activeFood.GetColliderSize<Vector3>());
                    break;
                case FoodType.Sausage:
                    _predictShotPoint.transform.position = PredictFoodPhysics.PredictFallPointByBoxRayCast<Vector3, Vector3>(transform.position, maxShotSpeedVector, ShotManager.Instance.GetShotAngleX(), activeFood.FoodType, activeFood.GetColliderSize<Vector3>());
                    break;
                default:
                    break;
            }
        }

        //void FixedUpdate()
        //{
        //}
        /// <summary>
        ///予測線の挙動を計算します。y方向のみの処理
        /// </summary>
        private void PredictPhysicsManage()
        {
            for (int i = 0; i < _predictLines.Count; i++)
            {
                //完全にプログラム計算形式にすることも可能
                var predictRb = _predictLines[i].predictLineRigidbody;
                var velocity = predictRb.velocity;
                velocity.y -= StageSceneManager.Instance.gravityAccelerationValue * Time.deltaTime;
                predictRb.velocity = velocity;
            }
        }
        /// <summary>
        /// 落下予測地点の変更を予測線を描くオブジェクトに対して渡す。XZ平面方向の変更
        /// </summary>
        private void ChangePredictFallPointOnXZ(Vector3 speedVector)
        {
            for (int i = 0; i < _predictLines.Count; i++)
            {
                //完全にプログラム計算形式にすることも可能
                var predictRb = _predictLines[i].predictLineRigidbody;
                var velocity = predictRb.velocity;
                velocity.x = speedVector.x;
                velocity.z = speedVector.z;
                predictRb.velocity = velocity;
            }
        }
        /// <summary>
        /// 予測線を管理
        /// </summary>
        private void PredictLineManage(Vector3 speedVector)
        {
            if (_predictTimeCounter >= _predictTimeInterval)
            {
                _predictTimeCounter = 0;
                PredictLine predictLine = InstantiatePredictLine(speedVector);
                predictLine.transform.parent = _predictLinesParent.transform;
                ManagePredictLinesList(predictLine);
                if (_predictLines.Count > 5)
                    Destroy(predictLine.gameObject);
            }
            else
                _predictTimeCounter += Time.deltaTime;

            for (int i = 0; i < _predictLines.Count; i++)
            {
                _predictLines[i].destroyTimeCounter += Time.deltaTime;
            }
        }
        /// <summary>
        /// 予測線の動的配列を管理。予測線の削除も行う。
        /// </summary>
        /// <param name="predictLine"></param>
        private void ManagePredictLinesList(PredictLine predictLine)
        {
            if (_predictLines.Count < _displayCountOfpredictLines)
                _predictLines.Add(predictLine);
            else
            {
                if (_predictLines[_destroyIndex].destroyTimeCounter >= _destroyTime)
                {
                    Destroy(_predictLines[_destroyIndex].gameObject);
                    _predictLines[_destroyIndex] = predictLine;
                    if (_destroyIndex < _displayCountOfpredictLines)
                    {
                        _destroyIndex++;
                        if (_destroyIndex == _displayCountOfpredictLines)
                        {
                            _destroyIndex = 0;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 予測線を生成 
        /// 予測線生成位置をアクティブプレイヤーの位置へ
        /// 予測線を飛ばす
        /// </summary>
        /// <remarks>予測線生成位置はturnControllerを参照</remarks>
        /// <returns></returns>
        private PredictLine InstantiatePredictLine(Vector3 speedVector)
        {
            var obj = Instantiate(_predictObjectPrefab);
            obj.transform.position = _instantiatePosition; //+ new Vector3(0, 0, 0);
            var predictLine = obj.GetComponent<PredictLine>();
            predictLine.predictLineRigidbody.velocity = speedVector;
            //obj.transform.parent = this.transform;
            return predictLine;
        }
        /// <summary>
        /// 現在シーン上の予測線を削除。ショット時に呼ばれる
        /// </summary>
        public void DestroyPredictLine()
        {
            //生成しすぎる不具合に対応 06/06
            var childCount = _predictLinesParent.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Destroy(_predictLinesParent.transform.GetChild(i).gameObject);
            }
            _predictLines.Clear();
        }
        /// <summary>
        /// 予測線生成位置をアクティブプレイヤーの位置へ 食材の中心にしたい 食材ごとに生成位置が変わる 生成時重力落下の可能性あり 更新必要
        /// </summary>
        public void SetPredictLineInstantiatePosition(Vector3 instantiatePosition)
        {
            _instantiatePosition = instantiatePosition;
            this.transform.position = instantiatePosition;
        }
        /// <summary>
        /// 落下予測地点の表示を切り替える
        /// </summary>
        /// <param name="activeState"></param>
        public void SetActivePredictShotPoint(bool activeState )
        {
            _predictShotPoint.SetActive(activeState);
        }

    }
}