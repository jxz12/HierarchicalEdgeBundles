using UnityEngine;

public class Screenshot : MonoBehaviour
{
    int idx=0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ScreenCapture.CaptureScreenshot($"{Screen.width}x{Screen.height} {idx++}.png");
        }
    }
}