using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Networking;


//Script to build new terrain, do not turn on unless using new terrain data
public class BuildTerrain : MonoBehaviour
{
    public Terrain terrain;
    public string csvFile = "bathymetry.csv";
    public float[,] elevation = new float[4097,4097];
    public float[,] normalized = new float[4097,4097];

    // Start is called before the first frame update
    void Start()
    {
        string filePath = Path.Combine(Application.dataPath, csvFile);
        string[] lines = File.ReadAllLines(filePath);

        for (int i = 0; i < lines.Length; i++)
        {
            string[] split = lines[i].Split(',');
            for (int j = 0; j < split.Length; j++)
            {
                elevation[i,j] = float.Parse(split[j]);
                normalized[i, j] = (elevation[i,j]+(float)10082.5537109375)/(float)16345.31689453125;
            }
        }

        terrain.terrainData.SetHeights(0, 0, normalized);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
