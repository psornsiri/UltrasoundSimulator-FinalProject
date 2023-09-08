using System;
using System.IO;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class SocketClient : MonoBehaviour
{
    private Thread clientThread;
    private TcpClient tcpClient;
    private bool isConnected = false;
    private float scale = 20.0f;
    private Vector3 pos_vect, rot_vect;

    private float getZ;
    public float posBelly;
    public bool touchBelly = false;
    public bool pos1 = true;
    public bool pos2, pos3, pos4 = false;
    public bool message = false;

    // Define a concurrent queue to store received pose values
    private ConcurrentQueue<(Vector3 position, Vector3 rotation)> poseQueue = new ConcurrentQueue<(Vector3, Vector3)>();


    // Use this for initialization
    void Start()
    {
        // Create and start a new thread for the socket connection to avoid blocking the main thread
        //clientThread = new Thread(ConnectToServer);
        //clientThread.IsBackground = true;
        //clientThread.Start();
    }

    void ConnectToServer()
    {
        try
        {
            // Connect to the C++ server
            tcpClient = new TcpClient("127.0.0.1", 12345);
            isConnected = true;

            // Receive data from the server
            using (NetworkStream stream = tcpClient.GetStream())
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (isConnected && tcpClient.Connected)
                {
                    // Read the pose values from the server
                    float posX = reader.ReadSingle();
                    float posY = reader.ReadSingle();
                    float posZ = reader.ReadSingle();
                    float rotX = reader.ReadSingle();
                    float rotY = reader.ReadSingle();
                    float rotZ = reader.ReadSingle();

                    //float forceX = reader.ReadSingle();
                    //float forceY = reader.ReadSingle();
                    //float forceZ = reader.ReadSingle();

                    // Process the received pose values (e.g., update a GameObject's position and rotation)
                    if (pos1)
                    {
                        pos_vect = new Vector3(-posX * scale, -posY * scale, -posZ * scale);
                        rot_vect = new Vector3(rotX, rotY, rotZ + 45f);
                    }
                    else if (pos2)
                    {
                        pos_vect = new Vector3(-posX * scale, posY * scale, posZ * scale);
                        rot_vect = new Vector3(rotX + 180f, -rotY, rotZ + 45f);
                    }
                    else if (pos3)
                    {
                        pos_vect = new Vector3(posX * scale, -posY * scale, posZ * scale);
                        rot_vect = new Vector3(-rotX + 180f, rotY, rotZ + 45f);
                    }
                    else if (pos4)
                    {
                        pos_vect = new Vector3(posX * scale, posY * scale, -posZ * scale);
                        rot_vect = new Vector3(-rotX, -rotY, rotZ + 45f);
                    }

                    //if (touchBelly && (pos_vect.z < posBelly))
                    //{
                    //    pos_vect.z = posBelly;
                    //    Debug.Log($"Collided at posZ: {pos_vect.z} or posBelly: {posBelly}");
                    //}

                    poseQueue.Enqueue((pos_vect, rot_vect));

                    Debug.Log($"p ({pos_vect.x}, {pos_vect.y}, {pos_vect.z}) | r ({rotX}, {rotY}, {rotZ})");
                    //Debug.Log($"p ({posX}, {posY}, {posZ}) | r ({rotX}, {rotY}, {rotZ})");
                    //Debug.Log($"p ({posX}, {posY}, {posZ}) | r ({rotX}, {rotY}, {rotZ}) | f ({forceX}, {forceY}, {forceZ})");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to server: {e.Message}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if there are any pose values in the queue
        if (poseQueue.TryDequeue(out (Vector3 position, Vector3 rotation) pose))
        {
            // Update the GameObject's position and rotation with the received pose values
            transform.localPosition = pose.position;
            transform.localRotation = Quaternion.Euler(pose.rotation);
        }

        if (message)
        {
            SendData(1000);
            message = false;
        }

        //if (touchBelly)
        //{
        //    transform.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        //    posBelly = transform.localPosition.z;
        //}
    }

    void FixedUpdate()
    {
        if (touchBelly)
        {
            transform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }


    // Cleanup when the script is disabled or the application is closed
    void OnDisable()
    {
        if (tcpClient != null)
        {
            isConnected = false;
            tcpClient.Close();
            tcpClient = null;
        }

        if (clientThread != null)
        {
            clientThread.Join();
            clientThread = null;
        }
    }    

    void OnCollisionEnter(Collision collision)
    {
        touchBelly = true;
        //posBelly = transform.localPosition.z;
    }

    private void OnCollisionExit(Collision collision)
    {
        touchBelly = false;
    }

    // Send haptic feedback to the server
    public void SendData(float sendCode)
    {
        string clientMessage = " ";

        if (tcpClient == null)
        {
            return;
        }
        try
        {
            NetworkStream stream = tcpClient.GetStream();
            if (stream.CanWrite)
            {
                clientMessage = sendCode.ToString();
                byte[] arrByte = System.Text.Encoding.ASCII.GetBytes(clientMessage);
                stream.Write(arrByte, 0, arrByte.Length);
                //Debug.Log("Sent message to haptic device ...");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error connecting to server: {e.Message}");
        }
    }
}

