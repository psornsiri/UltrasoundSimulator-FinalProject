using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SocketConnection();
    }

    public void SocketConnection()
    {
        var processInfo = new ProcessStartInfo
        {
            FileName = Application.dataPath + "/Gaming/sdk-3.15.0/bin/gravity.exe",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true
        };

        Process.Start(processInfo);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void LoadCreditScene()
    {
        SceneManager.LoadScene("CreditScene");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
