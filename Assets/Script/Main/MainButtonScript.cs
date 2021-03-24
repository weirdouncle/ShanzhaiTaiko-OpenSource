using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainButtonScript : MonoBehaviour
{
    public GameObject SelectedMask;
    public Animator Animator;
    public int Index;

    public bool Selected
    {
        get => selected;
        set
        {
            selected = value;
            SetSelected();
        }
    }

    private bool selected;


    private void SetSelected()
    {
        SelectedMask.SetActive(selected);
    }

    public void Click()
    {
#if UNITY_ANDROID
        SelectedMask.SetActive(true);
#endif
        Animator.speed = 10;
    }
}
