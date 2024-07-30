using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class PathFollower : MonoBehaviour
{

    public GameObject plotter;
    public OVRCameraRig cameraRig;
    public GameObject glider;
    public GameObject gliderArrow;
    public GameObject playerArrow;
    public float baseSpeed = 1.0f;
    public float speedUpFactor = 2.0f;
    public float slowDownFactor = 0.5f;

    private List<GameObject> pathPoints;
    private int currentPointIndex = 0;
    private bool isMoving = false;
    private bool isAttached = true;
    private bool pathPointsAvailable = false;
    private Vector3 offset = new Vector3(0f, 300f, 0f);


    //Method to get data from the plot2 script
    void attemptToGetPathPoints()
    {
        Plot2 plot2Script = plotter.GetComponent<Plot2>();

        pathPoints = plot2Script.getPoints();
        if (pathPoints != null && pathPoints.Count > 0)
        {
            pathPointsAvailable = true;
            isMoving = true;
            glider.transform.position = pathPoints[0].transform.position;
            gliderArrow.transform.position = glider.transform.position + offset;
            cameraRig.transform.position = pathPoints[0].transform.position;
            playerArrow.transform.position = cameraRig.transform.position + offset;
            glider.SetActive(!isAttached);
            gliderArrow.SetActive(!isAttached);
        }
    }


    //Toggles movement of the glider (start/stop)
    public void toggleMoving()
    {
        isMoving = !isMoving;
    }


    //Toggles attachment of player to the glider (attached/detached)
    public void toggleAttached()
    {
        isAttached = !isAttached;
        glider.SetActive(!isAttached);
        gliderArrow.SetActive(!isAttached);
    }


    //Gets the index of the current point that the glider is on
    public int getCurrentPointIndex()
    {
        return currentPointIndex;
    }


    // Start is called before the first frame update
    void Start()
    {
        attemptToGetPathPoints();
    }


    // Update is called once per frame
    void Update()
    {

        if (!pathPointsAvailable)
        {
            attemptToGetPathPoints();
            return;
        }

        Vector3 cameraForward = cameraRig.centerEyeAnchor.transform.forward;
        cameraForward.y = 0;

        //If glider is not moving, check if user toggled attachment, else skip the rest
        if (!isMoving)
        {
            playerArrow.transform.position = cameraRig.transform.position + offset;
            playerArrow.transform.rotation = Quaternion.LookRotation(cameraForward);
            if (isAttached)
            {
                cameraRig.transform.position = glider.transform.position;
            }
            return;
        }

        float distanceToNextPoint = Vector3.Distance(glider.transform.position, pathPoints[currentPointIndex].transform.position);

        if (distanceToNextPoint < 0.1f)
        {
            currentPointIndex++;
            if (currentPointIndex >= pathPoints.Count)
            {
                currentPointIndex = 0;
            }
        }
        
        //Speed adjustment
        float leftTriggerValue = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
        float rightTriggerValue = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        float triggerValue = Mathf.Max(leftTriggerValue, rightTriggerValue);
        float scale = Mathf.Lerp(0.5f, 10.0f, triggerValue);
        if (leftTriggerValue > rightTriggerValue)
        {
            scale = 1 / triggerValue;
        }
        

        //Glider movement
        Vector3 direction = (pathPoints[currentPointIndex].transform.position - glider.transform.position).normalized;
        float dynamicSpeed = Mathf.Lerp(baseSpeed, 0.1f * baseSpeed, distanceToNextPoint / 10.0f) * scale;
        glider.transform.position += direction * dynamicSpeed * Time.deltaTime;
        gliderArrow.transform.position = glider.transform.position + offset;

        //Glider rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        glider.transform.rotation = Quaternion.Slerp(glider.transform.rotation, targetRotation, dynamicSpeed * Time.deltaTime);
        Vector3 gliderForward = glider.transform.forward;
        gliderForward.y = 0;
        gliderArrow.transform.rotation = Quaternion.LookRotation(gliderForward);

        //Camera movement
        if (isAttached)
        {
            cameraRig.transform.position = glider.transform.position;
        }

        //Camera Rotation
        playerArrow.transform.position = cameraRig.transform.position + offset;
        playerArrow.transform.rotation = Quaternion.LookRotation(cameraForward);
    }

}