using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cooking
{
	public class SceneChanger : MonoBehaviour
	{
		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{
			if (Input.GetKeyDown(KeyCode.A))
			{
				var sceneName = SceneManager.GetActiveScene().name;
				SceneManager.LoadScene(sceneName);
			}

		}
	}

}

