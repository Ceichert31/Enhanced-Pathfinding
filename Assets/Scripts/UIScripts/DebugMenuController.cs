using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DebugMenuController : MonoBehaviour
{

    [SerializeField]
    private RectTransform debugMenu;

    [SerializeField]
    private float doTweenDuration = 0.3f;

    public void OpenDebugMenu(BoolEvent ctx)
    {
        if (ctx.Value)
        {
            debugMenu.DOAnchorPosY(400, doTweenDuration).SetEase(Ease.InOutCubic);
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            debugMenu.DOAnchorPosY(-510, doTweenDuration).SetEase(Ease.InOutCubic);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
