using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cooking.Stage
{
    /// <summary>
    /// 入力の衝突を防ぐ。現状スマホ対応していない Event…→うまく動かなかった RayCastの設定をすべて変更は手間 
    /// </summary>
    public class PreventTouchInputCollision : MonoBehaviour
    {
        /// <summary>
        /// サーチモードに戻る際のマウス入力(発射)の防止 衝突が起きる入力をするときの条件でこの変数を参照する(予定)
        /// </summary>
        public bool[] ShotInvalid
        {
            get { return _shotInvalid; }
        }
        private bool[] _shotInvalid;
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
            ShotButton
        }

        /// <summary>
        /// 衝突が起きるボタンを登録
        /// </summary>
        [SerializeField] RectTransform[] _preventTouchInputButtons;
        // Start is called before the first frame update
        void Start()
        {
            _shotInvalid = new bool[_preventTouchInputButtons.Length];
            foreach (var button in _preventTouchInputButtons)
            {
                Debug.Assert(button.GetComponent<Button>() != null,"ボタンをセットしてください。Buttonコンポーネントがありません。");
            }
        }

        // Update is called once per frame
        void Update()
        {
            #region サーチモードに戻る際のマウス入力(発射)の防止
            if (ShotManager.Instance.ShotModeProperty == ShotState.PowerMeterMode)
            {
                _shotInvalid[(int)ButtonName.ShotButton] = PreventInputCollision(_preventTouchInputButtons[(int)ButtonName.ShotButton]);
            }
            else
            {
                _shotInvalid[(int)ButtonName.ShotButton] = false;
            }
            #endregion
        }
        /// <summary>
        /// 登録されたボタンのRecttransformとマウスの座標がかぶっていたら、ボタンの入力を許可し、画面のタッチは禁止
        /// </summary>
        /// <param name="returnButton"></param>
        /// <returns></returns>
        private bool PreventInputCollision(RectTransform returnButton)
        {
            var returnButtonTransform = returnButton.transform.position;
            var returnButtonWidth = returnButton.sizeDelta.x;
            var returnButtonHeight = returnButton.sizeDelta.y;
            if (Input.mousePosition.x >= returnButtonTransform.x && Input.mousePosition.x <= returnButtonTransform.x + returnButtonWidth
                && Input.mousePosition.y >= returnButtonTransform.y && Input.mousePosition.y <= returnButtonTransform.y + returnButtonHeight)       //ボタンの座標とタッチする場所がかぶっていたら
            {
                return true;    //ボタンの入力を許可し、画面のタッチは禁止
            }
            else
            {
                return false;
            }
        }
    }
}
