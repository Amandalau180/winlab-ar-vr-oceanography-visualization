using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraPlayerFollower : MonoBehaviour
{

    public Transform player;
    public float height = 500f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = player.position;
        newPosition.y = height;
        transform.position = newPosition;

        //transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}
