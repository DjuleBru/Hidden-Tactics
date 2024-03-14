using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotsManager : MonoBehaviour
{
    int i = 0;

    void Update() {

        if (Input.GetKeyDown(KeyCode.X)) {
            i++;

            ScreenCapture.CaptureScreenshot("screenshot_" + i + ".png", 4);
            Debug.Log("A screenshot was taken!");
        }
    }
}
