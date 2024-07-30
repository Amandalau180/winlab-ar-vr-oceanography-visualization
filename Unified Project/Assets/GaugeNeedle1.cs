using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class GaugeNeedle : MonoBehaviour
{
    public GameObject needle; // Reference to the needle GameObject
    public GameObject plotterGameObject; // Reference to the GameObject with Plot2.cs attached
    public GameObject pathFollowerObject;

    /*
        public float testRotation = 45f; // Hardcoded rotation angle for testing

        void Update()
        {
            if (needle != null)
            {
                // Rotate the needle GameObject around the Z-axis
                needle.transform.Rotate(0, 0, -testRotation * Time.deltaTime, Space.Self); // Rotate needle
            }
            else
            {
                Debug.LogWarning("Needle GameObject is not assigned.");
            }
        }
    }*/


    public string variableName = "temperature"; // User-defined variable name to use for the gauge needle


    private Plot2 plotterScript; // Reference to the Plot2 script component
    private PathFollower pathScript;

    // Define the angles (in degrees) for your gauge points

   private float[] gaugeAngles = { -135f, -90f, -67.5f, -45f, -22.5f, 0f, 22.5f, 45f, 67.5f, 90f, 135f }; // Adjust angles as per your gauge design


    private float previousRotationAngle;
    private float needleAngle;

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
    }

    public void Update()
    {
        if (plotterScript != null && pathScript != null && needle != null)
        {
            UpdateNeedle();
        }
        else
        {
            Debug.LogWarning("One or more required references are missing in GaugeNeedle.");
        }
    }


   
    
     void UpdateNeedle()
    {
    Dictionary<string, List<float>> vals = plotterScript.getValues();
    List<float> tempvals = vals["temperature"];

    Dictionary<string, List<float>> minMax = plotterScript.getMinMax();
    List<float> tempExtremes = minMax["temperature"];

    float min = tempExtremes[0];
    float max = tempExtremes[1];

    int currIndex = pathScript.getCurrentPointIndex();
    float currVal = tempvals[currIndex];

    //Debug.Log("Current temperature: " + currVal);
    float dataVal = (currVal - min) / (max - min);


    dataVal = Mathf.Clamp01(dataVal);

    //Debug.Log("DataVal: " + dataVal);

    float currentAngle = needle.transform.rotation.eulerAngles.z;

    float newAngle = dataVal * -270f;


    float rot = newAngle - currentAngle;
    //Debug.Log("Angle to Rotate "+ rot);

    // Calculate the rotation difference
    float rotationDifference = newAngle - currentAngle;

     needle.transform.Rotate(0, 0, rot, Space.Self); // Rotate needle
    needle.transform.Rotate(0, 0, rotationDifference, Space.Self);





    /*if (needle == null)
    {
        Debug.LogWarning("Needle GameObject is not assigned.");
        return;
    }
    //needle.transform.rotation = Quaternion.Euler(0f, 0f, 0f);


    // Check if plotterScript and variableName are valid



    if (plotterScript != null && needle != null)
    {
       */     // Check if the variableName is one of the specified variables
              // if (variableName == "pressure" || variableName == "temperature" || variableName == "density")




    /*
    if (variableName == "temperature")
    {
        Debug.Log($"Variable {variableName} found. Proceeding with needle update.");
        Debug.Log($"Updating needle for variable: {variableName}");


        // Get the data value from Plot2 (assuming Plot2 has a method to provide needle data)
        dataValue = plotterScript.GetNeedleValue(variableName); // Call method GetNeedleData from Plot2.cs


    // Example: Determine min and max data values from your dataset
    //  float maxDataValue = Plot2.GetmaxDataValue;

    // float minDataValue = Plot2.GetmaxDataValue; // Replace with actual maximum value from your data

        float maxDataValue = plotterScript.GetmaxDataValue();
    float minDataValue = plotterScript.GetminDataValue();


    Debug.Log($"Data Value from Plot2 for {variableName}: {dataValue}");
    Debug.Log($"Max Data Value: {maxDataValue}, Min Data Value: {minDataValue}");

    // Normalize dataValue to range between 0 and 1
    float normalizedValue = Mathf.Clamp01((dataValue - minDataValue) / (maxDataValue - minDataValue));

    // Determine the current gauge index based on normalizedValue
    int gaugeIndex = Mathf.FloorToInt(normalizedValue * (gaugeAngles.Length - 1));
   // int gaugeIndex = Mathf.FloorToInt(dataValue * (gaugeAngles.Length - 1));
    Debug.Log($"Gauge Index: {gaugeIndex}");


    // Calculate the rotation angle based on the nearest gauge points
    float minAngle = gaugeAngles[gaugeIndex];
    float maxAngle = gaugeAngles[Mathf.Min(gaugeIndex + 1, gaugeAngles.Length - 1)];
    Debug.Log($"Min Angle: {minAngle}, Max Angle: {maxAngle}");


    float lerpValue = (normalizedValue - (float)gaugeIndex / (gaugeAngles.Length - 1)) * (gaugeAngles.Length - 1);
    float rotationAngle = Mathf.Lerp(minAngle, maxAngle, lerpValue);
    Debug.Log($"Rotation Angle: {rotationAngle}");

    // Debug logging for troubleshooting
    Debug.Log("Data Value: " + dataValue);
    Debug.Log("Normalized Value: " + normalizedValue);
    Debug.Log("Rotation Angle: " + rotationAngle);


    float currentAngle = needle.transform.rotation.eulerAngles.z;
     float newAngle = dataValue * -270f;

   // float newAngle = rotationAngle;

    float rot = newAngle - currentAngle;

    // Rotate the needle GameObject around the Z-axis
    // needle.transform.rotation = Quaternion.Euler(0f, 0f, rotationAngle);

     needle.transform.Rotate(0, 0, -rot, Space.Self); // Rotate needle
 previousRotationAngle = rotationAngle;



    Debug.Log("Needle rotated successfully.");


}
else
{
    Debug.LogWarning($"Variable {variableName} not supported for needle rotation.");
}
    */
    /*}
    else
    {
        Debug.LogWarning("Plotter script or needle GameObject is null. Cannot update needle.");
    }*/
    // Rotate the needle GameObject around the Z-axis


    // Ensure the needle doesn't go between 310 and 330 degrees
    /*
    if (needleAngle > 310f && needleAngle < 330f)
    {
        float midAngle = (310f + 330f) / 2f;
        needleAngle = (needleAngle < midAngle) ? 310f : 330f;
    }
    */


}

}

