using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking.Title
{
    public class TitleSceneManager : MonoBehaviour
    {
        [SerializeField] GameObject optionCanvas;
        [SerializeField] GameObject startButton;
        [SerializeField] UnityEngine.Playables.PlayableDirector director;
        // Start is called before the first frame update
        void Start()
        {
            AppTitle();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TouchStartButton()
        {
            StartCoroutine(LoadSceneCoroutine());
        }

        private void AppTitle()
        {
            StartCoroutine(AppTitleCoroutine());
        }

        IEnumerator LoadSceneCoroutine()
        {
            optionCanvas.SetActive(true);
            startButton.GetComponent<Animator>().SetTrigger("Start");
            startButton.GetComponent<UnityEngine.UI.Button>().enabled = false;
            Fader.FadeInAndOut(1.5f, 1.0f, 1.5f);
            yield return new WaitForSeconds(2.0f);
            SceneChanger.LoadSelectingScene(SceneName.SelectStage);
        }

        IEnumerator AppTitleCoroutine()
        {
            optionCanvas.SetActive(false);
            director.Play();

            while (director.state != UnityEngine.Playables.PlayState.Paused)
            {
                yield return null;
            }
            startButton.SetActive(true);
            optionCanvas.SetActive(true);
            yield break;
        }

    }
}
