using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPanel : MonoBehaviour
{

    public GameObject plotter;
    private Plot2 plot2Script;

    public GameObject pathFollower;
    private PathFollower pathFollowerScript;

    public OVRCameraRig ovrCameraRig;
    private VRFlyController VRFlyControllerScript;

    public GameObject userGuide;
    private ToggleVisibility toggleVisibilityScript;

    // Start is called before the first frame update
    void Start()
    {
        plot2Script = plotter.GetComponent<Plot2>();
        pathFollowerScript = pathFollower.GetComponent<PathFollower>();
        VRFlyControllerScript = ovrCameraRig.GetComponent<VRFlyController>();
        toggleVisibilityScript = userGuide.GetComponent<ToggleVisibility>();
    }

    // Update is called once per frame
    void Update()
    {

        //A button toggles variable display
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            plot2Script.toggleColoring();
        }

        //B button opens/closes menu
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            toggleVisibilityScript.toggleMenuText();
        }

        //X button starts/stops automatic path follower movement
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            pathFollowerScript.toggleMoving();
        }

        //Y button toggles the user and glider position
        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            pathFollowerScript.toggleAttached();
        }

        //Right joystick press toggles path thickness
        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
        {
            plot2Script.toggleThickness();
        }

    }
}
