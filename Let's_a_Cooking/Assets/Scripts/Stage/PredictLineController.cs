using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 予測線を生成
    /// 回転に合わせて飛ばす先の地点を計算して、予測線を飛ばす地点を変更する。
    /// </summary>
    public class PredictLineController : MonoBehaviour
    {
        /// <summary>
        /// 予測線表示用のプレハブオブジェクト。
        /// </summary>
        [SerializeField] GameObject _predictObjectPrefab;
        /// <summary>
        /// 現在シーン上に存在している予測線オブジェクトが入る。
        /// </summary>
        [SerializeField]private List<PredictLine> _predictLines = new List<PredictLine>();
        /// <summary>
        /// 予測線を飛ばす方向ベクトル。
        /// </summary>
        private Vector3 _predictLines_SpeedVector;
        /// <summary>
        /// 予測落下地点表示用プレハブオブジェクト。
        /// </summary>
        [SerializeField] GameObject _predictShotPointPrefab;
        /// <summary>
        /// 現在シーン上に存在している予測落下地点オブジェクトが入る。
        /// </summary>
        private GameObject _predictShotPoint;
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

        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得。
        /// </summary>
        public static PredictLineController Instance
        {
            get { return _instance; }
        }
        static PredictLineController _instance = null;

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

        void FixedUpdate()
        {
        }

        /// <summary>
        ///予測線の挙動を計算します。y方向のみの処理
        /// </summary>
        private void PredictPhysicsManage()
        {
            for (int i = 0; i < _predictLines.Count; i++)
            {
                //完全にプログラム計算形式にすることも可能 。
                var predictRb = _predictLines[i].predictLineRigidbody;
                var velocity = predictRb.velocity;
                velocity.y -= 9.81f * GameManager.Instance.gravityScale * Time.deltaTime;
                predictRb.velocity = velocity;
            }
        }
        /// <summary>
        /// 落下予測地点の変更を予測線を描くオブジェクトに対して渡す。XZ平面方向の変更。
        /// </summary>
        private void ChangePredictFallPointOnXZ()
        {
            for (int i = 0; i < _predictLines.Count; i++)
            {
                //完全にプログラム計算形式にすることも可能。
                var predictRb = _predictLines[i].predictLineRigidbody;
                var velocity = predictRb.velocity;
                velocity.x = _predictLines_SpeedVector.x;
                velocity.z = _predictLines_SpeedVector.z;
                predictRb.velocity = velocity;
            }
        }

        // Update is called once per frame
        void Update()
        {
            //予測線を管理。
            switch (UIManager.Instance.MainUIStateProperty)
            {
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.Start:
                    //予測線を飛ばす方向を取得。
                    _predictLines_SpeedVector = ShotManager.Instance.transform.forward * 20;
                    PredictLineManage();
                    PredictFallPoint();
                    break;
                case ScreenState.AngleMode:
                    //予測線を飛ばす方向を取得。
                    _predictLines_SpeedVector = ShotManager.Instance.transform.forward * 20;
                    PredictLineManage();
                    PredictFallPoint();
                    break;
                case ScreenState.SideMode:
                    //予測線を飛ばす方向を取得。
                    _predictLines_SpeedVector = ShotManager.Instance.transform.forward * 20;
                    PredictLineManage();
                    PredictFallPoint();
                    break;
                case ScreenState.LookDownMode:
                    //予測線を飛ばす方向を取得。
                    _predictLines_SpeedVector = ShotManager.Instance.transform.forward * 20;
                    PredictLineManage();
                    PredictFallPoint();
                    break;
                case ScreenState.PowerMeterMode:
                    //予測線を飛ばす方向を取得。
                    _predictLines_SpeedVector = ShotManager.Instance.transform.forward * 20;
                    PredictLineManage();
                    PredictFallPoint();
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
                case ScreenState.ChooseFood:
                    break;
                case ScreenState.Start:
                    //落下地点の変更を予測線にも伝える。
                    ChangePredictFallPointOnXZ();
                    PredictPhysicsManage();
                    break;
                case ScreenState.AngleMode:
                    //落下地点の変更を予測線にも伝える。
                    ChangePredictFallPointOnXZ();
                    PredictPhysicsManage();
                    break;
                case ScreenState.SideMode:
                    //落下地点の変更を予測線にも伝える。
                    ChangePredictFallPointOnXZ();
                    PredictPhysicsManage();
                    break;
                case ScreenState.LookDownMode:
                    //落下地点の変更を予測線にも伝える。
                    ChangePredictFallPointOnXZ();
                    PredictPhysicsManage();
                    break;
                case ScreenState.PowerMeterMode:
                    //落下地点の変更を予測線にも伝える。
                    ChangePredictFallPointOnXZ();
                    PredictPhysicsManage();
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
        /// <summary>
        /// 予測落下地点を計算
        /// </summary>
        private void PredictFallPoint()
        {
            #region 予測落下地点計算方法説明
            //距離(座標) = v0(初速度ベクトル) * 時間 + 1/2 * 重力加速度 * (時間)^2 //物体の大きさの分だけわずかにずれ  現在0.5fから発射
            // 0 = initialSpeedVector.y * t -1/2 *  9.81f * gravityScale * t * t
            // t ≠ 0 より tで割ると
            //1/2 * 9.81f * gravityScale * t  =  initialSpeedVector.y
            //t  =  initialSpeedVector.y /9.81f * gravityScale
            //滞空時間を算出。座標 y = 0 に戻ってくるまでにかかる時間。
            #endregion
            float t = _predictLines_SpeedVector.y / (0.5f * 9.81f * GameManager.Instance.gravityScale);
            //Debug.Log(t);
            //その時間ぶんだけxz平面上で初速のxzベクトル方向に等速直線運動させて、その運動が終わった地点を落下予測地点とする。ただし、落下地点は高さ0とする。
            Vector3 fallPoint = _instantiatePosition + new Vector3(_predictLines_SpeedVector.x, 0, _predictLines_SpeedVector.z) * t;
            _predictShotPoint.transform.position = fallPoint;
        }
        /// <summary>
        /// 予測線を管理
        /// </summary>
        private void PredictLineManage()
        {
            if (_predictTimeCounter >= _predictTimeInterval)
            {
                _predictTimeCounter = 0;
                PredictLine predictLine = InstantiatePredictLine();
                ManagePredictLinesList(predictLine);
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
        private PredictLine InstantiatePredictLine()
        {
            var obj = Instantiate(_predictObjectPrefab);
            obj.transform.position = _instantiatePosition; //+ new Vector3(0, 0, 0);
            var predictLine = obj.GetComponent<PredictLine>();
            predictLine.predictLineRigidbody.velocity = _predictLines_SpeedVector;
            //obj.transform.parent = this.transform;
            return predictLine;
        }
        /// <summary>
        /// 現在シーン上の予測線を削除。ショット時に呼ばれる
        /// </summary>
        public void DestroyPredictLine()
        {
            foreach (var predictLine in _predictLines)
            {
                Destroy(predictLine.gameObject);
            }
            _predictLines.Clear();
        }
        /// <summary>
        /// 予測線生成位置をアクティブプレイヤーの位置へ 食材の中心にしたい 食材ごとに生成位置が変わる 生成時重力落下の可能性あり 更新必要
        /// </summary>
        public void SetPredictLineInstantiatePosition(Vector3 instantiatePosition)
        {
            _instantiatePosition = instantiatePosition;
        }
    }
}
//                = turnController.foodStatuses[turnController.ActivePlayerIndex].transform.position; //+ new Vector3(0, 0, 0);
