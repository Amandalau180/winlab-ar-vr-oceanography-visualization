using UnityEngine;

public class ResizeObject : MonoBehaviour
{
    public float resizeSpeed = 1f;
    private OVRGrabbable ovrGrabbable;
    private bool isBeingGrabbed = false;

    void Start()
    {
        ovrGrabbable = GetComponent<OVRGrabbable>();
    }

    void Update()
    {
        if (ovrGrabbable.isGrabbed)
        {
            isBeingGrabbed = true;
        }
        else
        {
            isBeingGrabbed = false;
        }

        if (isBeingGrabbed)
        {
            float pinchAmountLeft = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
            float pinchAmountRight = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);

            Vector3 newScale = transform.localScale;
            if (pinchAmountRight > 0)
            {
                newScale += Vector3.one * pinchAmountRight * resizeSpeed * Time.deltaTime;
            }
            else if (pinchAmountLeft > 0)
            {

                if (newScale.x > 0.1f && newScale.y > 0.1f && newScale.z > 0.1f)
                {
                    newScale -= Vector3.one * pinchAmountLeft * resizeSpeed * Time.deltaTime;
                }
            }

            transform.localScale = newScale;
        }
    }
}
