using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using Newtonsoft.Json;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class Plot2 : MonoBehaviour
{
    public string dataset_id;                       //Dataset ID
    public string vars;                             //Variables of interest
    public int samplingRate = 5;                    //Sampling rate of data (lower is faster)
    public Gradient varGradient;                    //Default gradient if no gradient is found in colormaps
    public Gradient dateTimeGradient;               //Default gradient for datetime
    public OVRCameraRig cameraRig;                  //CameraRig object
    public int mapWidth;                            //Translates to longitude, dependent on terrain size, used to resize data to be accurate to the worldmap
    public int mapLength;                           //Translates to latitude, dependent on terrain size, used to resize data to be accurate to the worldmap
    public int mapHeight;                           //Translates to depth, dependent on terrain size, used to resize data to be accurate to the worldmap
    public bool downloadNewData;                    //Trans
    public float maxInterpolationLength = 20;       //Maximum length of connections between points (a number too large will result in unnatural connections)
    public string gradientFile = "cmocean.json";    //Gradient file name used to create gradients
    public Dictionary<string, string> colormapNames = new Dictionary<string, string>() {
        { "temperature", "thermal" },
        { "salinity", "haline" },
        { "chlorophyll_a", "algae" },
        { "pH", "matter" },
        { "oxygen_concentration_shifted_mgL", "deep" },
        { "conductivity", "haline" },
        { "density", "dense"}
    };
    public string currDisplay = null;
    public string currVarVal = null;
    public string currVarUnits = null;

    private string baseURL = "https://slocum-data.marine.rutgers.edu/erddap/tabledap/";                         //Base URL to access the ERDDAP database
    private string baseQuery = ".csv?time%2Clatitude%2Clongitude%2Cdepth%2C";                                   //Base query variables, essential for plotting 
    private Gradient currGradient;                                                                              //Current gradient being used in variable display
    private Dictionary<string, int> indexDict = new Dictionary<string, int>();                                  //Dictionary to store column number of variables (variable name: index)
    private Dictionary<string, List<float>> varsDict = new Dictionary<string, List<float>>();                   //Dictionary to store length along gradient of plotted points for each variable (variable name: gradient value)
    private Dictionary<string, List<float>> varsValDict = new Dictionary<string, List<float>>();                //Dictionary to store the original values of each variable, as read from the ERDDAP database (variable name: variable value)
    private Dictionary<string, List<float>> minmaxDict = new Dictionary<string, List<float>>();                 //Dictionary to store the min and max for each value, used in normalizing data (variable name: (min, max))
    private Dictionary<string, List<float>> latLongDepthDict = new Dictionary<string, List<float>>();           //Dictionary to store the latitude, longitude, and depth values, separate from other variables (variable name: variable value)
    private Dictionary<string, List<float>> latlongdepthMinmaxDict = new Dictionary<string, List<float>>();     //Dictionary to store the min and max for latitude, longitude, and depth values, separate from other variables (variable name: (min, max))
    private Dictionary<int, string> varsIntStringDict = new Dictionary<int, string>();                          //Dictionary to map ints to variable names, used for toggling between variable displays
    private Dictionary<string, Gradient> colormaps = new Dictionary<string, Gradient>();                        //Dictionary to store the gradients for each variable (variable name: gradient)
    private Dictionary<string, string> varsUnitDict = new Dictionary<string, string>();                         //Dictionary to store the units for each variable (variable name: unit)
    private List<string> varsList = new List<string>();                                                         //List to store all the variables of interest
    private List<double> dateTimeList = new List<double>();                                                     //List to store all datetime values
    private int colCounter = 0;                                                                                 //Counter to cycle through all variables of interest for variable display
    private List<GameObject> points = new List<GameObject>();                                                   //List to store all plotted data points
    private List<GameObject> conns = new List<GameObject>();                                                    //List to store all interpolated connections
    private bool isWall = false;                                                                                //Toggles display mode from path to wall


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


    //This method converts the real-world units into localized Unity units, accurate relative to the in-game map
    float[] convertUnits(float[] con)
    {
        con[indexDict["longitude"]] = ((con[indexDict["longitude"]])/180 * (mapWidth/2));
        con[indexDict["latitude"]] = ((con[indexDict["latitude"]])/90  * (mapLength/2));
        con[indexDict["depth"]] = -(con[indexDict["depth"]]) * ((float) mapHeight/10935);

        return con;
    }


    //This method reads in the gradients file and creates gradients for variables of interest
    void readGradients(string gradientFile)
    {
        colormaps = new Dictionary<string, Gradient>();
        string filePath = Path.Combine(Application.dataPath, gradientFile);
        string jsonString = File.ReadAllText(filePath);

        Dictionary<string, List<List<float>>> colormapData = new Dictionary<string, List<List<float>>>();

        try
        {
            colormapData = JsonConvert.DeserializeObject<Dictionary<string, List<List<float>>>>(jsonString);
        }
        catch (Exception e)
        {
            Debug.Log($"Error reading JSON file: {e.Message}");
        }

        foreach (var cmap in colormapData)
        {
            string colormapName = cmap.Key;
            List<List<float>> colorData = cmap.Value;
            List<Color> colors = new List<Color>();

            foreach (var color in colorData)
            {
                float r = color[0] / 255f;
                float g = color[1] / 255f;
                float b = color[2] / 255f;
                colors.Add(new Color(r, g, b));
            }

            Gradient gradient = CreateGradient(colors);
            colormaps[colormapName] = gradient;
        }
    }


    //This method is a helper method to create gradients for readGradients
    Gradient CreateGradient(List<Color> colors)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKeys = new GradientColorKey[colors.Count];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colors.Count];

        for (int i = 0; i < colors.Count; i++)
        {
            colorKeys[i] = new GradientColorKey(colors[i], i / (float)(colors.Count - 1));
            alphaKeys[i] = new GradientAlphaKey(1.0f, i / (float)(colors.Count - 1));
        }

        gradient.colorKeys = colorKeys;
        gradient.alphaKeys = alphaKeys;

        return gradient;
    }


    //This method calculates the colors of all points and connections and plots the initial version (datetime)
    IEnumerator plotData(string url, string path)
    {

        if (downloadNewData)
        {
            yield return StartCoroutine(downloadCSV(url, path));
        }

        string gradFilePath = Path.Combine(Application.dataPath, gradientFile);

        readGradients(gradFilePath);

        float d1 = 0, d2 = 0, d3 = 0;

        string filePath = Path.Combine(Application.dataPath, $"{dataset_id}.csv");

        string[] lines = File.ReadAllLines(filePath);

        //Find the index of the variables of interest, populates some dictionaries
        string[] header = lines[0].Split(",");
        string[] units = lines[1].Split(",");
        for (int i = 0; i < header.Length; i++)
        {
            header[i] = header[i].Trim();
        }
        List<string> timeLatLongDepth = new List<string> { "time", "latitude", "longitude", "depth" };
        foreach (string var in varsDict.Keys)
        {
            timeLatLongDepth.Add(var);
        }
        foreach (string colName in timeLatLongDepth)
        {
            indexDict[colName] = Array.IndexOf(header, colName);
            varsUnitDict[colName] = units[indexDict[colName]];
        }

        GameObject parent = GameObject.Find("Plotter");

        string[] line1 = lines[2].Split(",");

        //Finding min/max for datetime and lat/long/depth
        DateTime minTime = new DateTime();
        DateTime maxTime = new DateTime();
        bool dateTimeFound = false;
        List<string> latlongdepth = new List<string> { "latitude", "longitude", "depth" };

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

                foreach (var variable in latlongdepth)
                {
                    if (coords[indexDict[variable]] != "NaN")
                    {
                        float curr = float.Parse(coords[indexDict[variable]]);

                        if (!latlongdepthMinmaxDict.ContainsKey(variable))
                        {
                            latlongdepthMinmaxDict[variable] = new List<float> { curr, curr };
                        }
                        else
                        {
                            if (curr > latlongdepthMinmaxDict[variable][1])
                            {
                                latlongdepthMinmaxDict[variable][1] = curr;
                            }
                            if (curr < latlongdepthMinmaxDict[variable][0])
                            {
                                latlongdepthMinmaxDict[variable][0] = curr;
                            }
                        }
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
            }
        }

        //Plotting initial points
        latLongDepthDict = new Dictionary<string, List<float>> { { "latitude", new List<float>() }, { "longitude", new List<float>() }, { "depth", new List<float>() } };
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
                if (varsValDict.ContainsKey("time"))
                {
                    varsValDict["time"].Add((float)newTime.Ticks);
                }
                else
                {
                    varsValDict["time"] = new List<float> { (float)newTime.Ticks };
                }

                float[] newCoords = new float[coords.Length];
                for (int j = 1; j < coords.Length; j++)
                {
                    newCoords[j] = float.Parse(coords[j]);
                }

                latLongDepthDict["latitude"].Add(newCoords[indexDict["latitude"]]);
                latLongDepthDict["longitude"].Add(newCoords[indexDict["longitude"]]);
                latLongDepthDict["depth"].Add(newCoords[indexDict["depth"]]);

                float[] finalCoords = convertUnits(newCoords);
               
                foreach (string key in varsDict.Keys)
                {
                    float var1Val = float.Parse(coords[indexDict[key]]);
                    float var1Col = (var1Val - minmaxDict[key][0]) / (minmaxDict[key][1] - minmaxDict[key][0]);
                    varsDict[key].Add(var1Col);
                    if (varsValDict.ContainsKey(key))
                    {
                        varsValDict[key].Add(var1Val);
                    }
                    else
                    {
                        varsValDict[key] = new List<float> { var1Val };
                    }

                }

                //Draw initial points (datetime)
                GameObject pt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SphereCollider sphereCollider = pt.GetComponent<SphereCollider>();
                if (sphereCollider != null)
                {
                    sphereCollider.isTrigger = true;
                }

                Renderer rend = pt.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = dateTimeGradient.Evaluate((float)dateTimeCol);
                rend.material = mat;

                pt.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                pt.transform.position = new Vector3(finalCoords[indexDict["longitude"]], finalCoords[indexDict["depth"]], finalCoords[indexDict["latitude"]]);

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

                    cyl.transform.localScale = new Vector3(0.01f, distance / 2, 0.01f);

                    Vector3 direction = currPoint - prevPoint;
                    cyl.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

                    Renderer cylRend = cyl.GetComponent<MeshRenderer>();
                    cylRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));

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
        currDisplay = varsIntStringDict[0];
    }


    //Toggles path/wall display mode
    public void toggleThickness()
    {
        isWall = !isWall;
        if (isWall)
        {
            foreach (GameObject pt in points)
            {
                pt.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }

            foreach (GameObject cyl in conns)
            {
                Vector3 currentScale = cyl.transform.localScale;
                cyl.transform.localScale = new Vector3(0.25f, currentScale.y, 0.25f);
            }
        }
        if (!isWall)
        {
            foreach (GameObject pt in points)
            {
                pt.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            }

            foreach (GameObject cyl in conns)
            {
                Vector3 currentScale = cyl.transform.localScale;
                cyl.transform.localScale = new Vector3(0.01f, currentScale.y, 0.01f);
            }
        }
    }


    //This method allows the user to toggle the variable being displayed
    public void toggleColoring()
    {
        if (colCounter + 1> varsDict.Count)
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
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = dateTimeGradient.Evaluate((float)dateTimeList[index]);
                rend.material = mat;
            }
            index = 0;
            foreach (GameObject cyl in conns)
            {
                Renderer cylRend = cyl.GetComponent<MeshRenderer>();
                cylRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (cylRend.enabled == true)
                {
                    cylRend.material.color = dateTimeGradient.Evaluate((float)dateTimeList[index]);
                }
                index++;

            }

        }
        else
        {
            if (colormapNames.ContainsKey(varsIntStringDict[colCounter]))
            {
                currGradient = colormaps[colormapNames[varsIntStringDict[colCounter]]];
            }
            else
            {
                currGradient = varGradient;
            }

            int index = 0;
            foreach (GameObject go in points)
            {
                Renderer rend = go.GetComponent<MeshRenderer>();
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = currGradient.Evaluate((float)varsDict[varsIntStringDict[colCounter]][index]);
                rend.material = mat;
                index++;
            }
            index = 0;
            foreach (GameObject cyl in conns)
            {
                Renderer cylRend = cyl.GetComponent<MeshRenderer>();
                cylRend.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                if (cylRend.material.color.a != 0)
                {
                    cylRend.material.color = currGradient.Evaluate((float)varsDict[varsIntStringDict[colCounter]][index]);
                }
                index++;
            }
        }
        currDisplay = varsIntStringDict[colCounter];
    }


    //Getters to help other scripts access private variables
    public List<GameObject> getPoints()
    {
        return points;
    }

    public Dictionary<string, List<float>> getValues()
    {
        return varsValDict;
    }

    public Dictionary<string, string> getUnits()
    {
        return varsUnitDict;
    }

    public Dictionary<string, List<float>> getMinMax()
    {
        return minmaxDict;
    }

    public Dictionary<string, Gradient> getColorMaps()
    {
        return colormaps;
    }

    public Dictionary<string, List<float>> getLatLongDepthDict()
    {
        return latLongDepthDict;
    }


    // Start is called before the  frame update
    void Start()
    {
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
        varsIntStringDict[0] = "time";
        foreach (var key in varsList)
        {
            varsDict[key] = new List<float>();
            varsIntStringDict[index] = key;
            index++;
        }

        StartCoroutine(plotData(fileURL, $"Assets/{dataset_id}.csv"));
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    //Obsolete code for gauge, currently not in use

    //public float currentTemperature;
    //public float GetNeedleValue(string variableName)
    //{
    //    /*if (varsDict.ContainsKey("temperature") && varsDict["temperature"].Count > 0 &&
    //        varsDict.ContainsKey("density") && varsDict["density"].Count > 0 &&
    //        varsDict.ContainsKey("pressure") && varsDict["pressure"].Count > 0)
    //    {
    //        float temperatureValue = varsDict["temperature"][varsDict["temperature"].Count - 1];
    //        float densityValue = varsDict["density"][varsDict["density"].Count - 1];
    //        float pressureValue = varsDict["pressure"][varsDict["pressure"].Count - 1];

    //        Debug.Log($"Temperature: {temperatureValue}, Density: {densityValue}, Pressure: {pressureValue}");

    //        return temperatureValue; // Example: returning Temperature value
    //    }
    //    else
    //    {
    //        Debug.LogWarning("One or more variables (temperature, density, pressure) not found in varsDict or are empty. Ensure all data is properly loaded.");
    //        return 0f; // Default value if data is not available
    //    }
    //}


    //public float GetLatestTemperature()
    //{
    //    if (varsDict.ContainsKey("temperature") && varsDict["temperature"].Count > 0)
    //    {
    //        return varsDict["temperature"][varsDict["temperature"].Count - 1];
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Temperature data not available.");
    //        return 0f;
    //    }
    //}

    //public float GetLatestDensity()
    //{
    //    if (varsDict.ContainsKey("density") && varsDict["density"].Count > 0)
    //    {
    //        return varsDict["density"][varsDict["density"].Count - 1];
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Density data not available.");
    //        return 0f;
    //    }
    //}

    //public float GetLatestPressure()
    //{
    //    if (varsDict.ContainsKey("pressure") && varsDict["pressure"].Count > 0)
    //    {
    //        return varsDict["pressure"][varsDict["pressure"].Count - 1];
    //    }
    //    else
    //    {
    //        Debug.LogWarning("Pressure data not available.");
    //        return 0f;
    //    }
    //}*/ if (varsDict.ContainsKey(variableName) && varsDict[variableName].Count > 0)
    //    {
    //        switch (variableName)
    //        {
    //            case "temperature":
    //                return currentTemperature; // Replace with actual retrieval logic
    //            // Add cases for other variables if needed
    //            default:
    //                Debug.LogWarning($"Variable {variableName} not supported.");
    //                return 0f;
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"{variableName} data not available.");
    //        return 0f; // Default value if data is not available
    //    }
    //}

    ////void update()
    ////{
    ////    currentTemperature = Mathf.PingPong(Time.time * 10f, 100f); // Example to simulate temperature change
    ////}

}
