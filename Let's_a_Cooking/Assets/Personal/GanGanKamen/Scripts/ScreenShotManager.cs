using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            string fileName = "image" + Time.time + ".png";
            ScreenCapture.CaptureScreenshot("Assets/" + fileName);
            Debug.Log("screenshot");
        }
    }
}
