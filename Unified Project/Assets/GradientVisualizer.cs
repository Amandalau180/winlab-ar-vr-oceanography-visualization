using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GradientVisualizer : MonoBehaviour
{

    public Gradient gradient;
    public GameObject plotter;
    public Plot2 plotterScript;
    public RawImage image;
    public Dictionary<string, Gradient> colormaps;
    public bool gradientsAvailable = false;

    private void Start()
    {
        attemptToGetGradients();
        
    }

    private void attemptToGetGradients()
    {
        plotterScript = plotter.GetComponent<Plot2>();
        colormaps = plotterScript.getColorMaps();
        if (colormaps != null && colormaps.Count > 0)
        {
            gradientsAvailable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!gradientsAvailable)
        {
            attemptToGetGradients();
        }
        gradient = colormaps["thermal"];
        if (gradient == null || image == null)
        {
            Debug.LogWarning("Gradient or RawImage not assigned.");
            return;
        }

        Texture2D gradientTexture = new Texture2D(256, 50);
        for (int x = 0; x < gradientTexture.width; x++)
        {
            Color color = gradient.Evaluate((float)x / gradientTexture.width);
            for (int y = 0; y < gradientTexture.height; y++)
            {
                gradientTexture.SetPixel(x, y, color);
            }
        }
        gradientTexture.Apply();

        image.texture = gradientTexture;
    }
}
