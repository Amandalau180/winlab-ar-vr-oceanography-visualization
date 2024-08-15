using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class GaugeNeedle : MonoBehaviour
{
    public GameObject needle; // Reference to the needle GameObject
    public GameObject plotterGameObject; // Reference to the GameObject with Plot2.cs attached
    public GameObject pathFollowerObject;


    public string variableName = "temperature"; // User-defined variable name to use for the gauge needle


    private Plot2 plotterScript; // Reference to the Plot2 script component
    private PathFollower pathScript;

    // Define the angles (in degrees) for your gauge points

    public bool dataLoaded;

    public List<float> tempvals;
    public float max;
    public float min;

    

    void Start()
    {
        // Ensure plotterGameObject is assigned
        if (plotterGameObject != null)
        {
            // Attempt to get the Plot2 script component from plotterGameObject
            plotterScript = plotterGameObject.GetComponent<Plot2>();
            pathScript = pathFollowerObject.GetComponent<PathFollower>();

            if (plotterScript == null)
            {
                Debug.LogError("Plot2 script not found on plotterGameObject!");
            }
        }
        else
        {
            Debug.LogError("plotterGameObject reference not assigned in GaugeNeedle!");
        }
        if (needle == null)
        {
            Debug.LogError("Needle GameObject is not assigned.");
        }

        dataLoaded = false;

        needle.transform.Rotate(0, 0, 45, Space.Self);
    }

    //Loads in the data for the new variable when changed
    public void toggleData()
    {
        dataLoaded = false;
    }

    public void Update()
    {
        if (plotterScript != null && pathScript != null && needle != null)
        {
            if (plotterScript.currDisplay == "time")
            {
                //Gauge does not rotate when time is displayed
            }
            else if (!dataLoaded)
            {
                //checks if the data has been loaded before changing the position of the needle
                importData();
            }
            else
            {
                UpdateNeedle();
            }
            
        }
        else
        {
            Debug.LogWarning("One or more required references are missing in GaugeNeedle.");
        }
    }

    
    //Waits for web request before importing data
   void importData()
    {
        Dictionary<string, List<float>> vals = plotterScript.getValues();
        tempvals = vals[plotterScript.currDisplay];

        Dictionary<string, List<float>> minMax = plotterScript.getMinMax();
        List<float> extremes = minMax[plotterScript.currDisplay];

        min = extremes[0];
        max = extremes[1];

        if(tempvals != null)
        {
            dataLoaded = true;
        }
    }
    
    //Changes the position of the needle
    void UpdateNeedle()
    {
        //Gets the index of the point that the glider is at
        int currIndex = pathScript.getCurrentPointIndex();
        float newVal = tempvals[currIndex];

        //Converts the value into a ratio of the value to the range
        float newDataVal = (newVal - min) / (max - min);

        newDataVal = Mathf.Clamp01(newDataVal);

        //Converts the ratio into degrees
        float newAngle = 360 - (newDataVal * 270f);

        if (needle.transform.rotation.eulerAngles.z < (newAngle + 5) && needle.transform.rotation.eulerAngles.z > (newAngle - 5)){
            //If the needle is within 5 degrees of the desired angle, don't rotate
        }
        else if (needle.transform.rotation.eulerAngles.z < newAngle && needle.transform.rotation.eulerAngles.z < 360)
        {
            //Rotates counterclockwise if the value is greater than the desired angle
            needle.transform.Rotate(0, 0, 5, Space.Self);
        }
        else if (needle.transform.rotation.eulerAngles.z > newAngle && needle.transform.rotation.eulerAngles.z > 90)
        {
            //Rotates clockwise if the value is less than the desired angle
            needle.transform.Rotate(0, 0, -5, Space.Self);
        }


    }

}

