using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableWallBoundry : MonoBehaviour
{
    public GameObject wall;

    private void Awake()
    {
        wall.SetActive(gameObject.activeSelf);
    }
}
