using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Title
{
    public class TitleSceneManager : MonoBehaviour
    {
        [SerializeField] GameObject startButton;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TouchStartButton()
        {
            StartCoroutine(LoadSceneCoroutine());
        }

        IEnumerator LoadSceneCoroutine()
        {
            startButton.SetActive(false);
            Fader.FadeInAndOut(1.5f, 1.0f, 1.5f);
            yield return new WaitForSeconds(2.0f);
            SceneChanger.LoadSelectingScene(SceneName.SelectStage);
        }
    }
}
