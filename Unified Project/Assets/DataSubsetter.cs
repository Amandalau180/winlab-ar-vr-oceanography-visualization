using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using TMPro;

public class DataSubsetter : MonoBehaviour
{
    public GameObject plotter;
    public TMP_Text secondaryDisplay;

    private string fileName;
    private Dictionary<string, List<float>> varsValDict = new Dictionary<string, List<float>>();
    private Dictionary<string, string> varsUnitDict = new Dictionary<string, string>();
    private Dictionary<string, List<float>> latLongDepthDict = new Dictionary<string, List<float>>();
    private List<GameObject> points = new List<GameObject>();
    private string dataset_id;
    private List<int> selectedPoints = new List<int>();
    private bool dataAvailable;
    private Transform rightHandAnchor;
    private LineRenderer guideLine;


    //Method to get data from the plot2 script
    void attemptToGetData()
    {
        Plot2 plot2Script = plotter.GetComponent<Plot2>();

        varsValDict = plot2Script.getValues();
        varsUnitDict = plot2Script.getUnits();
        latLongDepthDict = plot2Script.getLatLongDepthDict();
        points = plot2Script.getPoints();
        dataset_id = plot2Script.dataset_id;
        if ((points != null && points.Count > 0) && (latLongDepthDict != null && latLongDepthDict.Count > 0))
        {
            dataAvailable = true;
        }
    }


    //Method to write data to CSV
    public void writeToCSV(string fileName)
    {
        selectedPoints.Sort();
        string filePath = Path.Combine(Application.dataPath, $"{fileName}_subset.csv");
        StringBuilder csvContent = new StringBuilder();

        string[] headers = new string[varsUnitDict.Count];
        string[] units = new string[varsUnitDict.Count];
        int headerIndex = 0;

        //Writing headers (variable names) and units
        foreach (var key in varsUnitDict.Keys)
        {
            headers[headerIndex] = key;
            units[headerIndex] = varsUnitDict[key];
            headerIndex++;
        }
        csvContent.AppendLine(string.Join(",", headers));
        csvContent.AppendLine(string.Join(",", units));

        //Writing data
        foreach (int rowIndex in selectedPoints)
        {
            List<string> row = new List<string>();

            foreach (string key in headers)
            {
                var values = "";
                if (key == "time")
                {
                    DateTime temp = new DateTime((long)varsValDict[key][rowIndex]);
                    values = temp.ToString();
                }
                else if (key == "latitude" || key == "longitude" || key == "depth")
                {
                    values = latLongDepthDict[key][rowIndex].ToString();
                } 
                else
                {
                    values = varsValDict[key][rowIndex].ToString();
                }
                row.Add(values);
            }
            csvContent.AppendLine(string.Join(",", row));
        }
        File.WriteAllText(filePath, csvContent.ToString());

        Debug.Log("Subset data sucessfully written");
    }


    // Start is called before the first frame update
    void Start()
    {
        rightHandAnchor = GameObject.Find("RightHandAnchor").transform;
        guideLine = rightHandAnchor.gameObject.AddComponent<LineRenderer>();
        guideLine.startWidth = 0.01f;
        guideLine.endWidth = 0.01f;
        guideLine.positionCount = 2;
        guideLine.material = new Material(Shader.Find("Sprites/Default"));
        guideLine.startColor = Color.red;
        guideLine.endColor = Color.red;

        attemptToGetData();
    }


    // Update is called once per frame
    void Update()
    {
        if (!dataAvailable)
        {
            secondaryDisplay.text = "Data not yet available";
            attemptToGetData();
            return;
        }

        secondaryDisplay.text = "No point selected";

        float triggerValue = OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger);

        //Anything under half-press shoots a guide laser but will not select points
        //Anything over half-press will select points hit by the guide laser
        if (triggerValue > 0.1f)
        {
            guideLine.enabled = true;
            guideLine.SetPosition(0, rightHandAnchor.position);
            guideLine.SetPosition(1, rightHandAnchor.position + rightHandAnchor.forward * 10.0f);
        }
        else
        {
            guideLine.enabled = false;
        }

        if (triggerValue >= .5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(rightHandAnchor.position, rightHandAnchor.forward, out hit))
            {
                int index = points.IndexOf(hit.collider.gameObject);
                secondaryDisplay.text = $"Selected point: {index}";
                if (!selectedPoints.Contains(index) && index != -1)
                {
                    selectedPoints.Add(index);
                }
            }
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            writeToCSV(dataset_id);
            secondaryDisplay.text = $"Writing to CSV...";
        }
    }
}
