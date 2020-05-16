using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    public class LanguageText : MonoBehaviour
    {
        [SerializeField] private string[] Texts; //0 日本語,1 英語,2 中国語
        private Language nowLanguage = OptionManager.language;
        private UnityEngine.UI.Text mainText;
        // Start is called before the first frame update
        void Start()
        {
            nowLanguage = OptionManager.language;
            mainText = GetComponent<UnityEngine.UI.Text>();
        }

        // Update is called once per frame
        void Update()
        {
            if(nowLanguage != OptionManager.language)
            {
                SetTextLanguage();
                nowLanguage = OptionManager.language;
            }
        }

        private void SetTextLanguage()
        {
            switch (OptionManager.language)
            {
                case Language.Japanese:
                    mainText.text = Texts[0];
                    break;
                case Language.English:
                    mainText.text = Texts[1];
                    break;
                case Language.Chinese:
                    mainText.text = Texts[2];
                    break;
            }
        }
    }
}


