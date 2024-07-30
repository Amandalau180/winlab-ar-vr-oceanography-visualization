//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System.IO;
//using System;
//using UnityEngine.Networking;
//using UnityEngine.XR;
//using UnityEngine.InputSystem;


//using UnityEngine.UI;
//using System.Linq;

//public class Plot2 : MonoBehaviour
//{
//    public string dataset_id;
//    public string var1;
//    public int samplingRate = 10;
//    public Gradient var1Gradient;
//    public Gradient dateTimeGradient;
//    public OVRCameraRig cameraRig;

//    private string baseURL = "https://slocum-data.marine.rutgers.edu/erddap/tabledap/";
//    private string baseQuery = ".csv?time%2Clatitude%2Clongitude%2Cdepth%2C";
//    private Dictionary<string, int> indexDict = new Dictionary<string, int>();
//    private List<GameObject> points = new List<GameObject>();
//    private List<float> var1List = new List<float>();
//    private List<double> dateTimeList = new List<double>();
//    private bool colorToggle = true;

//    IEnumerator downloadCSV(string url, string path)
//    {
//        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
//        {
//            yield return webRequest.SendWebRequest();

//            if (webRequest.result != UnityWebRequest.Result.Success)
//            {
//                Debug.Log(webRequest.result.ToString());
//                Debug.LogError("Failed to download: " + webRequest.error);
//            }
//            else
//            {
//                string csvData = webRequest.downloadHandler.text;
//                string directory = Path.GetDirectoryName(path);
//                if (!Directory.Exists(directory))
//                {
//                    Directory.CreateDirectory(directory);
//                }

//                File.WriteAllText(path, csvData);
//                Debug.Log("CSV saved to " + path);

//                AssetDatabase.Refresh();
//            }

//        }

//    }

//    float[] convertUnits(float[] con, float x, float z)
//    {
//        con[indexDict["latitude"]] = (con[indexDict["latitude"]] - x) * 111;
//        con[indexDict["depth"]] = -con[indexDict["depth"]] / 100;
//        con[indexDict["longitude"]] = (con[indexDict["longitude"]] - z) * 111;

//        return con;
//    }

//    IEnumerator plotData(string url, string path)
//    {
//        yield return StartCoroutine(downloadCSV(url, path));

//        float d1 = 0, d2 = 0, d3 = 0;

//        string filePath = Path.Combine(Application.dataPath, $"{dataset_id}.csv");

//        string[] lines = File.ReadAllLines(filePath);

//        //Finding the index of the variables of interest
//        string[] header = lines[0].Split(",");
//        for (int i = 0; i < header.Length; i++)
//        {
//            header[i] = header[i].Trim().ToLower();
//        }
//        List<string> timeLatLongDepth = new List<string> { "time", "latitude", "longitude", "depth", var1 };
//        foreach (string colName in timeLatLongDepth)
//        {
//            indexDict[colName] = Array.IndexOf(header, colName);
//        }

//        GameObject parent = GameObject.Find("Plotter");

//        string[] line1 = lines[2].Split(",");
//        float xpos = float.Parse(line1[indexDict["latitude"]]);
//        float depth = float.Parse(line1[indexDict["depth"]]);
//        float zpos = float.Parse(line1[indexDict["longitude"]]);

//        //Dynamic adapting of var1 variable
//        bool var1Found = false;
//        float var1Max = -1;
//        float var1Min = -1;
//        bool dateTimeFound = false;
//        DateTime minTime = new DateTime();
//        DateTime maxTime = new DateTime();


//        for (int k = 2; k < lines.Length; k += samplingRate)
//        {
//            string[] coords = lines[k].Split(",");

//            if ((coords[indexDict["latitude"]] != "NaN") && (coords[indexDict["longitude"]] != "NaN") && (coords[indexDict["depth"]] != "NaN"))
//            {
                
//                if (coords[indexDict[var1]] != "NaN")
//                {
//                    float curr = float.Parse(coords[indexDict[var1]]);
//                    if (!var1Found)
//                    {
//                        var1Max = curr;
//                        var1Min = curr;
//                        var1Found = true;
//                    }

//                    if (curr > var1Max)
//                    {
//                        var1Max = curr;
//                    }
//                    else if (curr < var1Min)
//                    {
//                        var1Min = curr;
//                    }
//                }

//                if (coords[indexDict["time"]] != "NaN")
//                {
//                    DateTime curr = DateTime.Parse(coords[indexDict["time"]]);
//                    if (!dateTimeFound)
//                    {
//                        minTime = curr;
//                        maxTime = curr;
//                        dateTimeFound = true;
//                    }

//                    if (curr > maxTime)
//                    {
//                        maxTime = curr;
//                    }
//                    else if (curr < minTime)
//                    {
//                        minTime = curr;
//                    }
//                }
//            }
                
//        }

//        for (int i = 2; i < lines.Length; i += samplingRate)
//        {
//            string[] coords = lines[i].Split(',');

//            if ((coords[indexDict["latitude"]] != "NaN") && (coords[indexDict["longitude"]] != "NaN") && (coords[indexDict["depth"]] != "NaN"))
//            {
//                DateTime newTime = DateTime.Parse(coords[indexDict["time"]]);
//                float var1Val = float.Parse(coords[indexDict[var1]]);
//                float[] newCoords = new float[coords.Length];
//                for (int j = 1; j < coords.Length; j++)
//                {
//                    newCoords[j] = float.Parse(coords[j]);
//                }

//                float[] finalCoords = convertUnits(newCoords, xpos, zpos);

//                GameObject pt = GameObject.CreatePrimitive(PrimitiveType.Sphere);

//                float var1Col = (var1Val - var1Min) / (var1Max - var1Min);

//                double dateTimeCol = (newTime - minTime) / (maxTime - minTime);

//                var1List.Add(var1Col);
//                dateTimeList.Add(dateTimeCol);

//                Renderer rend = pt.GetComponent<MeshRenderer>();
//                Material mat = new Material(Shader.Find("Standard"));
//                mat.color = var1Gradient.Evaluate(var1Col);
//                rend.material = mat;

//                pt.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
//                pt.transform.position = new Vector3(finalCoords[indexDict["latitude"]], finalCoords[indexDict["depth"]], finalCoords[indexDict["longitude"]]);

//                pt.transform.parent = parent.transform;

//                points.Add(pt);

//                d1 = finalCoords[indexDict["latitude"]]; d2 = finalCoords[indexDict["depth"]]; d3 = finalCoords[indexDict["longitude"]];  
//            }

//        }
//        //Teleports camera to the last point plotted
//        cameraRig.transform.position = new Vector3(d1, d2, d3);
//    }

//    void toggleColoring()
//    {
//        colorToggle = !colorToggle;

//        if (colorToggle)
//        {
//            int index = 0;
//            foreach (GameObject go in points)
//            {
//                Renderer rend = go.GetComponent<MeshRenderer>();
//                Material mat = new Material(Shader.Find("Standard"));
//                mat.color = var1Gradient.Evaluate(var1List[index]);
//                rend.material = mat;
//                index++;
//            }
//        }
//        else
//        {
//            int index = 0;
//            foreach (GameObject go in points)
//            {
//                Renderer rend = go.GetComponent<MeshRenderer>();
//                Material mat = new Material(Shader.Find("Standard"));
//                mat.color = dateTimeGradient.Evaluate((float) dateTimeList[index]);
//                rend.material = mat;
//                index++;
//            }
//        }
//    }

//    // Start is called before the  frame update
//    void Start()
//    {
//        //map is 20,0,10 (-10 to 10 and -5 to 5)

//        string fileURL = baseURL + dataset_id + baseQuery + var1;

//        StartCoroutine(plotData(fileURL, $"Assets/{dataset_id}.csv"));

//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
//        {
//            toggleColoring();
//        }
//    }
//}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class Plotting : MonoBehaviour
{
    public string dataset_id;
    public string vars;
    public int samplingRate = 10;
    public Gradient varGradient;
    public Gradient dateTimeGradient;
    public OVRCameraRig cameraRig;
    public string currDisplay = null;
    public float maxInterpolationLength = 20;

    private string baseURL = "https://slocum-data.marine.rutgers.edu/erddap/tabledap/";
    private string baseQuery = ".csv?time%2Clatitude%2Clongitude%2Cdepth%2C";
    private Dictionary<string, int> indexDict = new Dictionary<string, int>();
    private Dictionary<string, List<float>> varsDict = new Dictionary<string, List<float>>();
    private Dictionary<string, List<float>> minmaxDict = new Dictionary<string, List<float>>();
    private Dictionary<int, string> varsintStringDict = new Dictionary<int, string>();
    private List<String> varsList = new List<String>();
    private List<double> dateTimeList = new List<double>();
    private int colCounter = 0;
    private List<GameObject> points = new List<GameObject>();
    private List<GameObject> conns = new List<GameObject>();

    //This method downloads a user-specified CSV from the ERDDAP database
    IEnumerator downloadCSV(string url, string path)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.result.ToString());
                Debug.LogError("Failed to download: " + webRequest.error);
            }
            else
            {
                string csvData = webRequest.downloadHandler.text;
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(path, csvData);
                Debug.Log("CSV saved to " + path);

                AssetDatabase.Refresh();
            }

        }

    }

    //This method converts units so that points are accurate relative to each other
    float[] convertUnits(float[] con, float x, float z)
    {
        con[indexDict["latitude"]] = (con[indexDict["latitude"]] - x) * 111;
        con[indexDict["depth"]] = -con[indexDict["depth"]] / 100;
        con[indexDict["longitude"]] = (con[indexDict["longitude"]] - z) * 111;

        return con;
    }

    //This method calculates the colors of all points and connections and plots the initial version (datetime)
    IEnumerator plotData(string url, string path)
    {
        yield return StartCoroutine(downloadCSV(url, path));

        float d1 = 0, d2 = 0, d3 = 0;

        string filePath = Path.Combine(Application.dataPath, $"{dataset_id}.csv");

        string[] lines = File.ReadAllLines(filePath);

        //Find the index of the variables of interest
        string[] header = lines[0].Split(",");
        for (int i = 0; i < header.Length; i++)
        {
            header[i] = header[i].Trim().ToLower();
        }
        List<string> timeLatLongDepth = new List<string> { "time", "latitude", "longitude", "depth" };
        foreach (string var in varsDict.Keys)
        {
            timeLatLongDepth.Add(var);
        }
        foreach (string colName in timeLatLongDepth)
        {
            indexDict[colName] = Array.IndexOf(header, colName);
        }

        GameObject parent = GameObject.Find("Plotter");

        string[] line1 = lines[2].Split(",");
        float xpos = float.Parse(line1[indexDict["latitude"]]);
        float depth = float.Parse(line1[indexDict["depth"]]);
        float zpos = float.Parse(line1[indexDict["longitude"]]);

        //Finding min/max for datetime
        DateTime minTime = new DateTime();
        DateTime maxTime = new DateTime();
        bool dateTimeFound = false;

        for (int k = 2; k < lines.Length; k += samplingRate)
        {
            string[] coords = lines[k].Split(",");

            if ((coords[indexDict["latitude"]] != "NaN") && (coords[indexDict["longitude"]] != "NaN") && (coords[indexDict["depth"]] != "NaN"))
            {
                if (coords[indexDict["time"]] != "NaN")
                {
                    DateTime curr = DateTime.Parse(coords[indexDict["time"]]);
                    if (!dateTimeFound)
                    {
                        minTime = curr;
                        maxTime = curr;
                        dateTimeFound = true;
                    }

                    if (curr > maxTime)
                    {
                        maxTime = curr;
                    }
                    else if (curr < minTime)
                    {
                        minTime = curr;
                    }
                }
            }
        }

        //Find min/max for all other variables of interest
        for (int k = 2; k < lines.Length; k += samplingRate)
        {
            string[] coords = lines[k].Split(",");

            if ((coords[indexDict["latitude"]] != "NaN") && (coords[indexDict["longitude"]] != "NaN") && (coords[indexDict["depth"]] != "NaN"))
            {
                foreach (var variable in varsDict.Keys)
                {
                    if (coords[indexDict[variable]] != "NaN")
                    {
                        float curr = float.Parse(coords[indexDict[variable]]);

                        if (!minmaxDict.ContainsKey(variable))
                        {
                            minmaxDict[variable] = new List<float> { curr, curr };
                        }
                        else
                        {
                            if (curr > minmaxDict[variable][1])
                            {
                                minmaxDict[variable][1] = curr;
                            }
                            if (curr < minmaxDict[variable][0])
                            {
                                minmaxDict[variable][0] = curr;
                            }
                        }
                    }
                }

                if (coords[indexDict["time"]] != "NaN")
                {
                    DateTime currTime = DateTime.Parse(coords[indexDict["time"]]);

                    if (!dateTimeFound)
                    {
                        minTime = currTime;
                        maxTime = currTime;
                        dateTimeFound = true;
                    }
                    else
                    {
                        if (currTime > maxTime)
                        {
                            maxTime = currTime;
                        }
                        if (currTime < minTime)
                        {
                            minTime = currTime;
                        }
                    }
                }
            }
        }

        //Plotting initial points
        Vector3 prevPoint = Vector3.zero;
        bool firstPoint = true;
        for (int i = 2; i < lines.Length; i += samplingRate)
        {
            string[] coords = lines[i].Split(',');

            if ((coords[indexDict["latitude"]] != "NaN") && (coords[indexDict["longitude"]] != "NaN") && (coords[indexDict["depth"]] != "NaN"))
            {
                //Calculate and store colors for all variables of interest
                DateTime newTime = DateTime.Parse(coords[indexDict["time"]]);
                double dateTimeCol = (newTime - minTime) / (maxTime - minTime);
                dateTimeList.Add(dateTimeCol);

                float[] newCoords = new float[coords.Length];
                for (int j = 1; j < coords.Length; j++)
                {
                    newCoords[j] = float.Parse(coords[j]);
                }
                float[] finalCoords = convertUnits(newCoords, xpos, zpos);

                foreach (string key in varsDict.Keys)
                {
                    float var1Val = float.Parse(coords[indexDict[key]]);
                    float var1Col = (var1Val - minmaxDict[key][0]) / (minmaxDict[key][1] - minmaxDict[key][0]);
                    varsDict[key].Add(var1Col);
                }

                //Draw initial points (datetime)
                GameObject pt = GameObject.CreatePrimitive(PrimitiveType.Sphere);

                Renderer rend = pt.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = dateTimeGradient.Evaluate((float)dateTimeCol);
                rend.material = mat;

                pt.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                pt.transform.position = new Vector3(finalCoords[indexDict["latitude"]], finalCoords[indexDict["depth"]], finalCoords[indexDict["longitude"]]);

                pt.transform.parent = parent.transform;

                points.Add(pt);

                //Draw connections
                if (!firstPoint)
                {
                    Vector3 currPoint = pt.transform.position;
                    float distance = Vector3.Distance(prevPoint, currPoint);
                    GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

                    Vector3 midPoint = (prevPoint + currPoint) / 2;
                    cyl.transform.position = midPoint;

                    cyl.transform.localScale = new Vector3(0.05f, distance / 2, 0.05f);

                    Vector3 direction = currPoint - prevPoint;
                    cyl.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

                    Renderer cylRend = cyl.GetComponent<MeshRenderer>();
                    cylRend.material = new Material(Shader.Find("Standard"));

                    cyl.transform.parent = parent.transform;

                    conns.Add(cyl);
                    if (distance > maxInterpolationLength)
                    {
                        cylRend.enabled = false;
                    }
                    else
                    {
                        cylRend.material.color = dateTimeGradient.Evaluate((float)dateTimeCol);
                    }

                }

                //Update previousPoint to the current point's position
                prevPoint = pt.transform.position;
                firstPoint = false;

                d1 = finalCoords[indexDict["latitude"]]; d2 = finalCoords[indexDict["depth"]]; d3 = finalCoords[indexDict["longitude"]];
            }

        }
        //Teleports camera to the last point plotted
        cameraRig.transform.position = new Vector3(d1, d2, d3);
        currDisplay = varsintStringDict[0];
    }

    //This method allows the user to toggle the variable being displayed
    void toggleColoring()
    {
        if (colCounter + 1 > varsDict.Count)
        {
            colCounter = 0;
        }
        else
        {
            colCounter++;
        }

        if (colCounter == 0)
        {
            int index = 0;
            foreach (GameObject go in points)
            {
                Renderer rend = go.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = dateTimeGradient.Evaluate((float)dateTimeList[index]);
                rend.material = mat;
            }
            index = 0;
            foreach (GameObject cyl in conns)
            {
                Renderer cylRend = cyl.GetComponent<MeshRenderer>();
                cylRend.material = new Material(Shader.Find("Standard"));
                if (cylRend.enabled == true)
                {
                    cylRend.material.color = dateTimeGradient.Evaluate((float)dateTimeList[index]);
                }
                index++;

            }

        }
        else
        {
            int index = 0;
            foreach (GameObject go in points)
            {
                Renderer rend = go.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = varGradient.Evaluate(varsDict[varsintStringDict[colCounter]][index]);
                rend.material = mat;
                index++;
            }
            index = 0;
            foreach (GameObject cyl in conns)
            {
                Renderer cylRend = cyl.GetComponent<MeshRenderer>();
                cylRend.material = new Material(Shader.Find("Standard"));
                if (cylRend.material.color.a != 0)
                {
                    cylRend.material.color = varGradient.Evaluate(varsDict[varsintStringDict[colCounter]][index]);
                }
                index++;
            }
        }
        currDisplay = varsintStringDict[colCounter];
    }

    public List<GameObject> getPoints()
    {
        return points;
    }

    // Start is called before the  frame update
    void Start()
    {
        //map is 20,0,10 (-10 to 10 and -5 to 5)

        varsList = vars.Split(",").ToList();
        string fileURL = baseURL + dataset_id + baseQuery;

        for (int i = 0; i < varsList.Count(); i++)
        {
            varsList[i] = varsList[i].Trim();
            fileURL += (varsList[i] + "%2C");
        }

        if (fileURL.EndsWith("%2C"))
        {
            fileURL = fileURL.TrimEnd("%2C".ToCharArray());
        }

        int index = 1;
        varsintStringDict[0] = "time";
        foreach (var key in varsList)
        {
            varsDict[key] = new List<float>();
            varsintStringDict[index] = key;
            index++;
        }

        StartCoroutine(plotData(fileURL, $"Assets/{dataset_id}.csv"));



    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            toggleColoring();
        }
    }

    internal float GetCurrentValue(Gauge.VariableType currentVariable)
    {
        throw new NotImplementedException();
    }

}
