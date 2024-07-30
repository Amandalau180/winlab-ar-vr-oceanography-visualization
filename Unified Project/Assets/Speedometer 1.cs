using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gauge : MonoBehaviour
{
    // Public variables

  public float maxTemp = 100.0f;
    public float maxPressure = 0.0f;
    public float maxSalinity = 0.0f;
    
   
    public float minArrowAngle = -90.0f;
    public float maxArrowAngle = 90.0f;



    // UI elements
    public Text valueLabel; // The label that displays the value
    public RectTransform arrow; // The arrow in the gauge

    // Enum for different variable types
    public enum VariableType
    {
        Temperature,
        Pressure,
        Salinity
    }

    // Current variable type
    public VariableType currentVariable = VariableType.Temperature;

    private Plotting plotter; // Reference to the Plotting script

    private void Start()
    {
        plotter = FindObjectOfType<Plotting>();
        if (plotter == null)
        {
            Debug.LogError("Plotting script not found in the scene.");
        }
    }

    private void Update()
    {
        UpdateDisplay();
    }

    public void SetVariableType(int variableType)
    {
        currentVariable = (VariableType)variableType;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (plotter == null) return;

        float currentValue = plotter.GetCurrentValue(currentVariable);
        float maxValue = GetMaxValue(currentVariable);

        // Update value label
        if (valueLabel != null)
            valueLabel.text = currentValue.ToString("F2") + " units";

        // Update arrow rotation
        if (arrow != null)
        {
            float normalizedValue = Mathf.Clamp01(currentValue / maxValue);
            float targetAngle = Mathf.Lerp(minArrowAngle, maxArrowAngle, normalizedValue);
            arrow.localEulerAngles = new Vector3(0, 0, targetAngle);
        }
    }

    private float GetMaxValue(VariableType variable)
    {
        switch (variable)
        {
            case VariableType.Temperature:
                return maxTemp;
            case VariableType.Pressure:
                return maxPressure;
            case VariableType.Salinity:
                return maxSalinity;
            default:
                return 1.0f; // Default value, should not reach here ideally
        }
    }
}

