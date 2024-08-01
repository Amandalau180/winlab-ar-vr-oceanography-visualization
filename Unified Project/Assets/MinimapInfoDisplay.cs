using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinimapInfoDisplay : MonoBehaviour
{
    public TMP_Text infoText;
    public GameObject plotter;
    public GameObject pathFollower;

    private PathFollower pathFollowerScript;
    private Plot2 plot2Script;
    private Dictionary<string, List<float>> pathVals = new Dictionary<string, List<float>>();
    private Dictionary<string, string> units = new Dictionary<string, string>();

    // Start is called before the first frame update
    void Start()
    {
        infoText.text = "Loading...";
        plot2Script = plotter.GetComponent<Plot2>();
        pathVals = plot2Script.getValues();
        pathFollowerScript = pathFollower.GetComponent<PathFollower>();
        units = plot2Script.getUnits();
    }

    // Update is called once per frame
    void Update()
    {
        if (plot2Script.currDisplay != null)
        {
            if (plot2Script.currDisplay == "time")
            {
                DateTime tempTime = new DateTime((long)pathVals[plot2Script.currDisplay][pathFollowerScript.getCurrentPointIndex()]);
                infoText.text = "Now Displaying: \n" + plot2Script.currDisplay + "\n" + tempTime + " " + units[plot2Script.currDisplay];
            }
            else
            {
                infoText.text = "Now Displaying: \n" + plot2Script.currDisplay + "\n" + pathVals[plot2Script.currDisplay][pathFollowerScript.getCurrentPointIndex()] + " " + units[plot2Script.currDisplay];
            }
        }
    }
}
