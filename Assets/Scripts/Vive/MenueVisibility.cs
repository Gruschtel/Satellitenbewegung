using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;
using System;

public class MenueVisibility : MonoBehaviour
{
    [SerializeField] private GameObject rightHand;
    [SerializeField] private Canvas canvas;
    // Start is called before the first frame update
    void Start()
    {
    }

    void Update()
    {
        if(ViveInput.GetPress(HandRole.RightHand, ControllerButton.DPadUp)) openMenue();
        if(ViveInput.GetPress(HandRole.RightHand, ControllerButton.DPadDown)) closeMenue();
    }

    private void openMenue()
    {
        canvas.gameObject.SetActive(true);
    }
    
    private void closeMenue()
    {
        canvas.gameObject.SetActive(false);
    }
}
