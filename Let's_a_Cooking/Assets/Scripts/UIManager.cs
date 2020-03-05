using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Cooking.Stage
{
	public class UIManager : MonoBehaviour
	{
		//shotPowerゲージを取得
		[SerializeField]
		private Slider powerGage;

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			//shotPowerをゲージに反映
			//powerGage.value = shotPower;

		}
	}

}
