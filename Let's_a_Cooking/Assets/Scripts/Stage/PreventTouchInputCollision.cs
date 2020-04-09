using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Stage
{
    /// <summary>
    /// 入力の衝突を防ぐ。 入力を行うクラス Eventにあったとは…
    /// </summary>
    public class PreventTouchInputCollision : MonoBehaviour
    {
        /// <summary>
        /// サーチモードに戻る際のマウス入力(発射)の防止 衝突が起きる入力をするときの条件でこの変数を参照する(予定)
        /// </summary>
        public bool ShotInvalid
        {
            get { return _shotInvalid; }
        }
        private bool _shotInvalid;
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
        /// 衝突が起きるボタンを登録
        /// </summary>
        [SerializeField] RectTransform[] _preventTouchInputButtons;
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            #region サーチモードに戻る際のマウス入力(発射)の防止
            if (ShotManager.Instance.ShotModeProperty == ShotState.PowerMeterMode)
            {
                var returnButton = UIManager.Instance.returnButton;
                var returnButtonTransform = returnButton.transform.position;
                var returnButtonWidth = returnButton.sizeDelta.x;
                var returnButtonHeight = returnButton.sizeDelta.y;
                if (Input.mousePosition.x >= returnButtonTransform.x && Input.mousePosition.x <= returnButtonTransform.x + returnButtonWidth
                    && Input.mousePosition.y >= returnButtonTransform.y && Input.mousePosition.y <= returnButtonTransform.y + returnButtonHeight)
                {
                    _shotInvalid = true;
                }
                else
                {
                    _shotInvalid = false;
                }
            }
            else
            {
                _shotInvalid = false;
            }
            #endregion
        }
    }
}
