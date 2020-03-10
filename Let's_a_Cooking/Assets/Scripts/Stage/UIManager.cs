using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Cooking.Stage
{
	public class UIManager : MonoBehaviour
	{
        /// <summary>
        /// ショット前のカメラの回転は、ショットの向きを決めるオブジェクトのRotationのyを参照します(左右)。x(高さ方向の回転)は参照しません。
        /// ショット前のカメラ動作はmainカメラで行います。
        /// ショットオブジェクトは一つのみで、各プレイヤーで使いまわします。
        /// </summary>
        Shot shotObject;
        //shotPowerゲージを取得
        [SerializeField]
		private Slider powerGage;

        #region インスタンスへのstaticなアクセスポイント
        /// <summary>
        /// このクラスのインスタンスを取得します。
        /// </summary>
        public static UIManager Instance
        {
            get { return instance; }
        }
        static UIManager instance = null;

        /// <summary>
        /// Start()より先に実行されます。
        /// </summary>
        private void Awake()
        {
            instance = this;
        }
        #endregion

        // Start is called before the first frame update
        void Start()
		{
            shotObject = FindObjectOfType<Shot>();
        }

        // Update is called once per frame
        void Update()
		{
			//shotPowerをゲージに反映
			powerGage.value = shotObject.shotPower;
		}
	}

}
