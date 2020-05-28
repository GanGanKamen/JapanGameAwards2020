using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cooking
{
    public class LanguageText : MonoBehaviour
    {
        [SerializeField] private string[] Texts; //0 日本語,1 英語,2 中国語
        private Language nowLanguage = OptionParamater.language;
        private UnityEngine.UI.Text mainText;
        // Start is called before the first frame update
        void Start()
        {
            nowLanguage = OptionParamater.language;
            mainText = GetComponent<UnityEngine.UI.Text>();
            SetTextLanguage();
        }

        // Update is called once per frame
        void Update()
        {
            if(nowLanguage != OptionParamater.language)
            {
                SetTextLanguage();
                nowLanguage = OptionParamater.language;
            }
        }

        private void SetTextLanguage()
        {
            switch (OptionParamater.language)
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


