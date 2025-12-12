using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        //Win game
        gameObject.GetComponent<sceneManager>().SceneWin();
    }
}
