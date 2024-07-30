using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using TMPro;

public class ToggleVisibility : MonoBehaviour
{

    public TMP_Text menuText;
    
    // Start is called before the first frame update
    void Start()
    {

    }

    public void toggleMenuText()
    {
        menuText.enabled = !menuText.enabled;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
