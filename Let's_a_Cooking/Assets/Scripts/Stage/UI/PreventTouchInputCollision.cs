using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Touches;

namespace Cooking.Stage
{
    /// <summary>
    /// 画面タッチ入力・ボタンの衝突を防ぐ。現状スマホ対応していない Eventうまく動かなかった RayCastの設定をすべて変更は手間 
    /// </summary>
    public class PreventTouchInputCollision : MonoBehaviour
    {
        /// <summary>
        /// 現状(05/28)ではMiddleCenterとBottomLeftのみ可能
        /// </summary>
        enum AnchorMode
        {
            TopLeft, TopCenter, TopRight,
            MiddleLeft, MiddleCenter, MiddleRight,
            BottomLeft, BottonCenter, BottomRight
        }
        AnchorMode[] _anchorMode;
        /// <summary>
        /// 画面タッチとボタンタッチの衝突が起きる際、ボタンの入力をするときこの変数を参照する
        /// </summary>
        public bool[] TouchInvalid
        {
            get { return _touchInvalid; }
        }
        private bool[] _touchInvalid;
        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得
        /// </summary>
        public static PreventTouchInputCollision Instance
        {
            get { return _instance; }
        }
        static PreventTouchInputCollision _instance = null;

        /// <summary>
        /// Start()より先に実行
        /// </summary>
        private void Awake()
        {
            _instance = this;
        }
        #endregion

        /// <summary>
        /// 入力衝突を防ぎたいボタンの種類を登録
        /// </summary>
        public enum ButtonName
        {
            OptionButton
        }
        private Vector2 _buttomLeftPosition = Vector2.zero;
        private Vector2 _middleCenterPosition = new Vector2(0.5f,0.5f);
        /// <summary>
        /// 衝突が起きるボタンを登録
        /// </summary>
        [SerializeField] RectTransform[] _preventTouchInputButtons = null;
        // Start is called before the first frame update
        void Start()
        {
            if(_preventTouchInputButtons.Length > 0)
            {
                _touchInvalid = new bool[_preventTouchInputButtons.Length];
                _anchorMode = new AnchorMode[_preventTouchInputButtons.Length];
                for (int i = 0; i < _preventTouchInputButtons.Length; i++)
                {
                    var button = _preventTouchInputButtons[i];
                    if (button.GetComponent<Button>() == null)
                    {
                        Debug.Assert(button.GetComponent<Button>() != null, "ボタンをセットしてください。Buttonコンポーネントがありません。");
                        continue;
                    }
                    if (button.pivot == _buttomLeftPosition)
                    {
                        _anchorMode[i] = AnchorMode.BottomLeft;
                    }
                    else if (button.pivot == _middleCenterPosition)
                    {
                        _anchorMode[i] = AnchorMode.MiddleCenter;
                    }
                    else
                    {
                        Debug.Log("pivotが真ん中または左下ではないボタンです ボタン入力とタッチの衝突が起こります");
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_preventTouchInputButtons.Length > 0)
            {
                #region 順番決め画面オプション開閉時のタッチ入力の防止
                if (UIManager.Instance.MainUIStateProperty == ScreenState.DecideOrder)
                {
                    _touchInvalid[(int)ButtonName.OptionButton] = PreventInputCollision(ButtonName.OptionButton);
                }
                else
                {
                    _touchInvalid[(int)ButtonName.OptionButton] = false;
                }
                #endregion
            }
        }
        /// <summary>
        /// 登録されたボタンのRecttransformとマウスの座標がかぶっていたら、ボタンの入力を許可し、画面のタッチは禁止
        /// </summary>
        /// <param name="returnButton"></param>
        /// <returns></returns>
        private bool PreventInputCollision(ButtonName buttonName)
        {
            var button = _preventTouchInputButtons[(int)buttonName];
            var returnButtonTransform = button.transform.position;
            var returnButtonWidth = button.sizeDelta.x * button.localScale.x;
            var returnButtonHeight = button.sizeDelta.y * button.localScale.y;
            var touchPosition = TouchInput.GetPosition();
            switch (_anchorMode[(int)buttonName])
            {
                case AnchorMode.TopLeft:
                    break;
                case AnchorMode.TopCenter:
                    break;
                case AnchorMode.TopRight:
                    break;
                case AnchorMode.MiddleLeft:
                    break;
                case AnchorMode.MiddleCenter:
                    if (touchPosition.x >= returnButtonTransform.x - returnButtonWidth / 2 && touchPosition.x <= returnButtonTransform.x + returnButtonWidth / 2
    && touchPosition.y >= returnButtonTransform.y - returnButtonHeight / 2 && touchPosition.y <= returnButtonTransform.y + returnButtonHeight / 2)       //ボタンの座標とタッチする場所がかぶっていたら
                    {
                        return true;    //ボタンの入力を許可し、画面のタッチは禁止
                    }
                    else
                    {
                        return false;
                    }
                case AnchorMode.MiddleRight:
                    break;
                case AnchorMode.BottomLeft:
                    if (touchPosition.x >= returnButtonTransform.x && touchPosition.x <= returnButtonTransform.x + returnButtonWidth
    && touchPosition.y >= returnButtonTransform.y && touchPosition.y <= returnButtonTransform.y + returnButtonHeight)       //ボタンの座標とタッチする場所がかぶっていたら
                    {
                        return true;    //ボタンの入力を許可し、画面のタッチは禁止
                    }
                    else
                    {
                        return false;
                    }
                case AnchorMode.BottonCenter:
                    break;
                case AnchorMode.BottomRight:
                    break;
                default:
                    break;
            }
            return false;
        }
    }
}
