///////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2001-2022 Force Dimension, Switzerland.
//  All Rights Reserved.
//
//  Force Dimension SDK 3.15.0
//
///////////////////////////////////////////////////////////////////////////////


#include <stdio.h>
#include <stdlib.h>
#define _USE_MATH_DEFINES
#include <math.h>

#include "Eigen/Eigen"
using namespace Eigen;

#include "dhdc.h"
#include "drdc.h"

#include <iostream>
#include <fstream>

#include <winsock2.h>
#include <ws2tcpip.h>

#pragma comment(lib, "Ws2_32.lib")

#define REFRESH_INTERVAL  0.1   // sec

#define KP    100.0
#define KVP    10.0
#define MAXF    4.0
#define KR      0.3
#define KWR     0.02
#define MAXT    0.1
#define KG    100.0
#define KVG     5.0
#define MAXG    1.0

extern "C" __declspec(dllexport) void SetInitPos()
{
    double rightPose[DHD_MAX_DOF] = { 0.0537, 0.0493, 0.0299,  // translation
                                     0.0365, 2.1492, 0.0139,  // rotation (joint angles)
                                     0.0 };          // gripper

    double nullPose[DHD_MAX_DOF] = { -0.115565, -0.06869, -0.0776301,  // translation
                                     0.0, 0.0, 0.0,  // rotation (joint angles)
                                     0.0 };          // gripper

    int iResult = drdMoveTo(nullPose);
    if (iResult < 0)
    {
        std::cout << "failed to set initial position: " << dhdErrorGetLastStr() << std::endl;
    }
    std::cout << "Move to Pose: " << iResult << std::endl;
}

extern "C" __declspec(dllexport) void ForceFeedback(float dist)
{
    double p[DHD_MAX_DOF];
    double r[3][3];

    double v[DHD_MAX_DOF];
    double f[DHD_MAX_DOF];
    double normf, normt;
    int    res;

    // Eigen objects (mapped to the arrays above)
    Map<Vector3d> position(&p[0], 3);
    Map<Vector3d> force(&f[0], 3);
    Map<Vector3d> torque(&f[3], 3);
    Map<Vector3d> velpos(&v[0], 3);
    Map<Vector3d> velrot(&v[3], 3);
    Matrix3d center;
    Matrix3d rotation;

    // center of workspace
    //center.setIdentity();
    center(0, 0) = -0.656034;
    center(0, 1) = -0.753392;
    center(0, 2) = -0.0449412;
    center(1, 0) = 0.753445;
    center(1, 1) = -0.657229;
    center(1, 2) = 0.0192655;
    center(2, 0) = -0.0440512;
    center(2, 1) = -0.0212219;
    center(2, 2) = 0.998804;

    // object properties
    double Stiffness = 100.0;
    Vector3d dir = { 0.0, 0.0, 1.0 };

    /*Vector3d hapticPos = { p[0], p[1], p[2] };
    Vector3d bellyPos = { p[0], p[1], p[2] - (double)dist };
    Vector3d dir = (hapticPos - bellyPos).normalized();*/

    // get position/orientation/gripper and update Eigen rotation matrix
    drdGetPositionAndOrientation(&(p[0]), &(p[1]), &(p[2]),
        &(p[3]), &(p[4]), &(p[5]),
        &(p[6]), r);
    for (int i = 0; i < 3; i++) rotation.row(i) = Vector3d::Map(&r[i][0], 3);

    // get position/orientation/gripper velocity
    drdGetVelocity(&(v[0]), &(v[1]), &(v[2]),
        &(v[3]), &(v[4]), &(v[5]),
        &(v[6]));

    // compute base centering force
    force = -KP * position;

    // compute wrist centering torque
    AngleAxisd deltaRotation(rotation.transpose() * center);
    torque = rotation * (KR * deltaRotation.angle() * deltaRotation.axis());

    // compute gripper centering force
    f[6] = -KG * (p[6] - 0.015);

    // scale force to a pre-defined ceiling
    if ((normf = force.norm()) > MAXF) force *= MAXF / normf;

    // scale torque to a pre-defined ceiling
    if ((normt = torque.norm()) > MAXT) torque *= MAXT / normt;

    // scale gripper force to a pre-defined ceiling
    if (f[6] > MAXG) f[6] = MAXG;
    if (f[6] < -MAXG) f[6] = -MAXG;

    // add damping
    force -= KVP * velpos;
    torque -= KWR * velrot;
    f[6] -= KVG * v[6];
    
    Vector3d hapticForce = dist * Stiffness * dir;

    //int iResult = dhdSetForceAndGripperForce(hapticForce(0), hapticForce(1), hapticForce(2), 0.0);

    int iResult = drdSetForceAndTorqueAndGripperForce(hapticForce(0) + f[0], hapticForce(1) + f[1], hapticForce(2) + f[2],  // force
                                                      f[3], f[4], f[5],  // torque
                                                      0.0);           // gripper force
    if (iResult < 0)
    {
        std::cout << "failed to set haptic feedback: " << dhdErrorGetLastStr() << std::endl;
    }
}

extern "C" __declspec(dllexport) void fixedPosition(bool isLock)
{
    drdRegulatePos(isLock);
    drdRegulateRot(isLock);

    double p[DHD_MAX_DOF];
    double r[3][3];
    // get position/orientation/gripper and update Eigen rotation matrix
    drdGetPositionAndOrientation(&(p[0]), &(p[1]), &(p[2]),
        &(p[3]), &(p[4]), &(p[5]),
        &(p[6]), r);

    for (int x = 0; x < 3; x++)
    {
        for (int y = 0; y < 3; y++)
        {
            std::cout << r[x][y] << " ";
        }
        std::cout << std::endl;
    }
}

extern "C" __declspec(dllexport) void openDevice()
{
    int done;

    // message
    printf("Force Dimension - Server Connection %s\n", dhdGetSDKVersionStr());
    printf("Copyright (C) 2001-2022 Force Dimension\n");
    printf("All Rights Reserved.\n\n");

    // open the first available device
    if (drdOpen() < 0) {
        printf("error: cannot open device (%s)\n", dhdErrorGetLastStr());
        dhdSleep(2.0);
        done = -1;
    }

    // print out device identifier
    if (!drdIsSupported()) {
        printf("unsupported device\n");
        printf("exiting...\n");
        dhdSleep(2.0);
        drdClose();
        done = -1;
    }
    printf("%s haptic device detected\n\n", dhdGetSystemName());

    // center of workspace
    double nullPose[DHD_MAX_DOF] = { 0.0, 0.0, 0.0,  // translation
                                     0.0, 0.0, 0.0,  // rotation (joint angles)
                                     0.0 };          // gripper

    // perform auto-initialization
    if (!drdIsInitialized() && drdAutoInit() < 0) {
        printf("error: auto-initialization failed (%s)\n", dhdErrorGetLastStr());
        dhdSleep(2.0);
        done = -1;
    }
    else if (drdStart() < 0) {
        printf("error: regulation thread failed to start (%s)\n", dhdErrorGetLastStr());
        dhdSleep(2.0);
        done = -1;
    }

    // move to center
    //drdMoveTo(nullPose);

    // request a null force (only gravity compensation will be applied)
    // this will only apply to unregulated axis
    drdSetForceAndTorqueAndGripperForce(0.0, 0.0, 0.0,  // force
                                        0.0, 0.0, 0.0,  // torque
                                        0.0);           // gripper force

    // disable all axis regulation (but leave regulation thread running)
    drdRegulatePos(false);
    drdRegulateRot(false);
    drdRegulateGrip(false);
}


extern "C" __declspec(dllexport) double(&returnPose(double(&arr)[6]))[6]
{
    double px, py, pz;
    double oa, ob, og;

    int    done  = 0;
    double t0    = dhdGetTime ();
    double t1    = t0;
    int    count = 0;

    // haptic loop
    while (!done) {

        // display refresh rate and position at 10Hz
        t1 = drdGetTime ();
        if ((t1-t0) > REFRESH_INTERVAL) {

            // retrieve/compute information to display
            double freq = (double)count/(t1-t0)*1e-3;
            count = 0;
            t0 = t1;

            // retrieve position
            if (dhdGetPositionAndOrientationDeg(&px, &py, &pz, &oa, &ob, &og) < DHD_NO_ERROR) {
                printf("error: cannot read position (%s)\n", dhdErrorGetLastStr());
                done = 1;
            }

            arr[0] = px;
            arr[1] = py;
            arr[2] = pz;
            arr[3] = oa;
            arr[4] = ob;
            arr[5] = og;

            // display status;
            //printf("p (%+0.03f %+0.03f %+0.03f %+0.03f %+0.03f %+0.03f) m \r", arr[0], arr[1], arr[2], arr[3], arr[4], arr[5]);

            return arr;
        }
    }

    // stop regulation
    drdStop ();

    // close the connection
    printf ("cleaning up... \n");
    drdClose ();

    // happily exit
    printf ("\ndone.\n");
    done = 0;
}

// Seperate thread for listening to incoming message
DWORD WINAPI ReadingThread(LPVOID param)
{
    char buf[1024];
    int buflen = 1024;
    bool posLock = false;
    SOCKET socket = (SOCKET)param;

    do
    {
        int iResult = recv(socket, buf, buflen, 0);
        if (iResult > 0)
        {
            // retrieve the message from client
            std::string iMessage(buf, sizeof(buf));
            float msg = stof(iMessage);

            if (msg == 1000)
            {
                posLock = !posLock;
                fixedPosition(posLock);
            }
            else if (msg == 2000)
            {
                SetInitPos();
            }
            else
            {
                ForceFeedback(msg);
            }
            //std::cout << "Message from client: " << msg << std::endl;
        }
        else if (iResult == 0)
        {
            std::cout << "Connection closing..." << std::endl;
            break;
        }
        else
        {
            std::cout << "recv failed with error: " << WSAGetLastError() << std::endl;
            break;
        }

        memset(buf, 0, buflen);

    } while (true);

    return 1;
}

// Main thread for sending message
int main(int argc, char** argv)
{
    int result;
    double arr[6];
    openDevice();

    SOCKET listenSocket = INVALID_SOCKET;
    SOCKET clientSocket = INVALID_SOCKET;

    // Initialize Winsock
    WSADATA wsaData;
    result = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (result != 0)
    {
        std::cerr << "WSAStartup failed: " << result << std::endl;
        return 1;
    }

    // Set up server address information
    struct addrinfo* addr = nullptr, hints;
    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    hints.ai_flags = AI_PASSIVE;

    // Resolve the local address and port to be used by the server
    result = getaddrinfo(nullptr, "12345", &hints, &addr);
    if (result != 0)
    {
        std::cerr << "getaddrinfo failed: " << result << std::endl;
        WSACleanup();
        return 1;
    }

    // Create a SOCKET object to listen for client connections
    listenSocket = socket(addr->ai_family, addr->ai_socktype, addr->ai_protocol);
    if (listenSocket == INVALID_SOCKET)
    {
        std::cerr << "socket failed: " << WSAGetLastError() << std::endl;
        freeaddrinfo(addr);
        WSACleanup();
        return 1;
    }

    // Bind the socket to the local address and port
    result = bind(listenSocket, addr->ai_addr, static_cast<int>(addr->ai_addrlen));
    if (result == SOCKET_ERROR)
    {
        std::cerr << "bind failed: " << WSAGetLastError() << std::endl;
        freeaddrinfo(addr);
        closesocket(listenSocket);
        WSACleanup();
        return 1;
    }

    // Free the address information structure as it's no longer needed
    freeaddrinfo(addr);

    // Set the socket to listen for incoming connections
    result = listen(listenSocket, SOMAXCONN);
    if (result == SOCKET_ERROR)
    {
        std::cerr << "listen failed: " << WSAGetLastError() << std::endl;
        closesocket(listenSocket);
        WSACleanup();
        return 1;
    }

    std::cout << "Waiting for a client connection..." << std::endl;

    // Accept a client connection
    clientSocket = accept(listenSocket, nullptr, nullptr);
    if (clientSocket == INVALID_SOCKET)
    {
        std::cerr << "accept failed: " << WSAGetLastError() << std::endl;
        closesocket(listenSocket);
        WSACleanup();
        return 1;
    }

    std::cout << "Incoming connection from client" << std::endl;

    // Close the listening socket as it's no longer needed
    closesocket(listenSocket);

    // Create a listening thread for receiving message
    HANDLE listenThread;
    DWORD dwThreadID;

    listenThread = CreateThread(NULL, 0, &ReadingThread, (void*)clientSocket, 0, &dwThreadID);
    if (!listenThread)
    {
        std::cerr << "createthread failed: " << WSAGetLastError() << std::endl;
        WSACleanup();
        return 1;
    }

    // Main thread sending pose info to Unity
    do
    {
        returnPose(arr);

        float posX = arr[0], posY = arr[1], posZ = arr[2], rotX = arr[3], rotY = arr[4], rotZ = arr[5];

        send(clientSocket, reinterpret_cast<char*>(&posX), sizeof(posX), 0);
        send(clientSocket, reinterpret_cast<char*>(&posY), sizeof(posY), 0);
        send(clientSocket, reinterpret_cast<char*>(&posZ), sizeof(posZ), 0);
        send(clientSocket, reinterpret_cast<char*>(&rotX), sizeof(rotX), 0);
        send(clientSocket, reinterpret_cast<char*>(&rotY), sizeof(rotY), 0);
        send(clientSocket, reinterpret_cast<char*>(&rotZ), sizeof(rotZ), 0);

    } while (true);

    // Cleanup winsock
    closesocket(clientSocket);
    WSACleanup();
}
