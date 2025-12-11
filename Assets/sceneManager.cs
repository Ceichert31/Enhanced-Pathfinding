using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SceneWin()
    {
        SceneManager.LoadScene("WIN");
    }
    public void SceneLose()
    {
        SceneManager.LoadScene("LOSE");
    }
    public void ScenePlay()
    {
        SceneManager.LoadScene("PLAY");
    }
    public void SceneMenu()
    {
        SceneManager.LoadScene("MENU");
    }
}
