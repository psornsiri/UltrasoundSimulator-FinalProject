using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbeCollision : MonoBehaviour
{
    public float collisionThreshold;

    private SocketClient socket;

    // Start is called before the first frame update
    void Start()
    {
        socket = GameObject.FindGameObjectWithTag("Client").GetComponent<SocketClient>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 rayOrigin = transform.position;
        // set offset from the origin point
        rayOrigin.y += 0.08f;
        rayOrigin.z += 0.05f;
        //collisionThreshold = (transform.localScale.y / 2f) + (float)0.01;

        RaycastHit hit;
        Ray landingRay = new Ray(rayOrigin, -transform.forward);

        Debug.DrawRay(rayOrigin, -transform.forward * collisionThreshold);

        if (Physics.Raycast(landingRay, out hit, collisionThreshold))
        {
            if (hit.collider.gameObject.tag == "Abdominal")
            {
                float dist = collisionThreshold - Vector3.Distance(rayOrigin, hit.point);
                float hapticDist = (collisionThreshold - hit.distance);
                socket.SendData(hapticDist);
                socket.touchBelly = true;
                Debug.Log("dist = " + hapticDist);
            }
            else
            {
                socket.SendData(0);
            }
        }
        else
        {
            socket.SendData(0);
            socket.touchBelly = false;
        }
    }
}
