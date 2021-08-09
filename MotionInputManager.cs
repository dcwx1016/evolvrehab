using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class MotionInputManager : MonoBehaviour
{
    protected static MotionInputManager instance = null;
    // Bool to keep track of whether Kinect has been initialized
    protected bool miInitialized = false;
    private Vector3 rightHandposition;
    private Vector3 leftHandposition;
    private Socket udpClient;
    protected Vector3[] joints;

    // Start is called before the first frame update
    void Awake()
    {
        // set the singleton instance
        instance = this;
        joints = new Vector3[25];
        udpClient = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        new Thread(ReceiveMessage) { IsBackground = true }.Start();
    }

    public static MotionInputManager Instance
    {
        get
        {
            return instance;
        }
    }

    public static bool IsKinectInitialized()
    {
        return instance != null ? instance.miInitialized : false;
    }

    public bool IsInitialized()
    {
        return true;
    }

    /// <summary>
    /// Gets the joint orientation of the specified user.
    /// </summary>
    /// <returns>The joint rotation.</returns>
    /// <param name="userId">User ID</param>
    /// <param name="joint">Joint index</param>
    /// <param name="flip">If set to <c>true</c>, this means non-mirrored rotation</param>
    public Quaternion GetJointOrientation(int joint)
    {
        //if (userManager.dictUserIdToIndex.ContainsKey(userId))
        //{
        //    int index = userManager.dictUserIdToIndex[userId];

        //    if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
        //    {
        //        if (flip)
        //            return alTrackedBodies[index].joint[joint].normalRotation;
        //        else
        //            return alTrackedBodies[index].joint[joint].mirroredRotation;
        //    }
        //}
        return Quaternion.identity;
    }

    public Vector3 GetJointPointing(int joint)
    {
        return joints[joint];
    }

    public Vector3 GetUserPosition()
    {
        //if (userManager.dictUserIdToIndex.ContainsKey(userId))
        //{
        //    int index = userManager.dictUserIdToIndex[userId];

        //    if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
        //    {
        //        return alTrackedBodies[index].position;
        //    }
        //}

        return new Vector3(0,0,4);
    }

    public Vector3 GetJointPosColorOverlay( int joint, int sensorIndex, Camera camera, Rect imageRect)
    {
        //if (userManager.dictUserIdToIndex.ContainsKey(userId) && camera != null)
        //{
        //    int index = userManager.dictUserIdToIndex[userId];

        //    if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
        //    {
        //        if (joint >= 0 && joint < (int)KinectInterop.JointType.Count)
        //        {
        //            KinectInterop.JointData jointData = alTrackedBodies[index].joint[joint];
        //            Vector3 posJointRaw = jointData.kinectPos;

        //            return GetJointPosColorOverlay(posJointRaw, sensorIndex, camera, imageRect);
        //        }
        //    }
        //}

        return Vector3.zero;
    }
    /// <summary>
    /// Gets the user position in Kinect coordinate system, in meters.
    /// </summary>
    /// <returns>The user kinect position.</returns>
    /// <param name="userId">User ID</param>
    /// <param name="applyDepthScale">Whether to apply the sensor space scale or not</param>
    public Vector3 GetUserKinectPosition(bool applySpaceScale)
    {
        //if (userManager.dictUserIdToIndex.ContainsKey(userId))
        //{
        //    int index = userManager.dictUserIdToIndex[userId];

        //    if (index >= 0 && index < trackedBodiesCount && alTrackedBodies[index].bIsTracked)
        //    {
        //        Vector3 userKinectPos = alTrackedBodies[index].kinectPos;

        //        if (applySpaceScale && btSensorIndex >= 0 && btSensorIndex < sensorDatas.Count)
        //        {
        //            Vector3 spaceScale = sensorDatas[btSensorIndex].sensorSpaceScale;
        //            return new Vector3(userKinectPos.x * spaceScale.x, userKinectPos.y * spaceScale.y, userKinectPos.z);
        //        }
        //        else
        //        {
        //            return userKinectPos;
        //        }
        //    }
        //}

        return Vector3.zero;
    }
    public int GetPrimaryBodySensorIndex()
    {
        return 0;
    }

    public bool IsJointTracked(int joint)
    {
        //if (dictUserIdToIndex.ContainsKey(userId))
        //{
        //    int index = dictUserIdToIndex[userId];

        //    if (index >= 0 && index < sensorData.bodyCount &&
        //        bodyFrame.bodyData[index].bIsTracked != 0)
        //    {
        //        if (joint >= 0 && joint < sensorData.jointCount)
        //        {
        //            KinectInterop.JointData jointData = bodyFrame.bodyData[index].joint[joint];

        //            return ignoreInferredJoints ? (jointData.trackingState == KinectInterop.TrackingState.Tracked) :
        //                (jointData.trackingState != KinectInterop.TrackingState.NotTracked);
        //        }
        //    }
        //}
        return true;
    }

    public Vector3 GetJointMiPosition(int joint)
    {
        //if (dictUserIdToIndex.ContainsKey(userId))
        //{
        //    int index = dictUserIdToIndex[userId];

        //    if (index >= 0 && index < sensorData.bodyCount &&
        //       bodyFrame.bodyData[index].bIsTracked != 0)
        //    {
        //        if (joint >= 0 && joint < sensorData.jointCount)
        //        {
        //            KinectInterop.JointData jointData = bodyFrame.bodyData[index].joint[joint];
        //            return jointData.kinectPos;
        //        }
        //    }
        //}
        if(joint == 0) return rightHandposition;
        if (joint == 1) return leftHandposition;
        return Vector3.zero;
    }

    public string GetRightHandState()
    {
        //if (dictUserIdToIndex.ContainsKey(userId))
        //{
        //    int index = dictUserIdToIndex[userId];

        //    if (index >= 0 && index < sensorData.bodyCount &&
        //        bodyFrame.bodyData[index].bIsTracked != 0)
        //    {
        //        return bodyFrame.bodyData[index].rightHandState;
        //    }
        //}
        return "closed";
    }

    public string GetLeftHandState()
    {
        //if (dictUserIdToIndex.ContainsKey(userId))
        //{
        //    int index = dictUserIdToIndex[userId];

        //    if (index >= 0 && index < sensorData.bodyCount &&
        //        bodyFrame.bodyData[index].bIsTracked != 0)
        //    {
        //        return bodyFrame.bodyData[index].rightHandState;
        //    }
        //}
        return "";
    }

    public bool IsUserDetected()
    {
        //TODO: add verification.
        return true;
    }

    public bool IsUserTracked()
    {
        //TODO: add verification.
        return true;
    }

    public void UpdatePosition(string val)
    {
        string[] xy = val.Split(","[0]);
        int xPosition = int.Parse(xy[0]) / 2;
        int yPosition = 720 - int.Parse(xy[1]);
        //Debug.Log(val[0]);
        //Debug.Log(val[1]);
        rightHandposition = new Vector3(xPosition, yPosition, 0);
        leftHandposition = new Vector3(int.Parse(xy[2]) / 2, 720 - int.Parse(xy[3]), 0);
    }

    public void UpdateOrientation(string val)
    {
        string[] xyz = val.Split(";"[0]);
        if(xyz.Length > 7)
        {
            string[] shoulder = xyz[0].Split(","[0]);
            string[] upperArm = xyz[1].Split(","[0]);
            string[] leftShoulder = xyz[2].Split(","[0]);
            string[] leftUpperArm = xyz[3].Split(","[0]);
            string[] rightUpperLeg = xyz[4].Split(","[0]);
            string[] rightLowerLeg = xyz[5].Split(","[0]);
            string[] leftUpperLeg = xyz[6].Split(","[0]);
            string[] leftLowerLeg = xyz[7].Split(","[0]);
            string[] chest = xyz[8].Split(","[0]);
            string[] rightHand = xyz[9].Split(","[0]);
            string[] leftHand = xyz[10].Split(","[0]);
            string[] rightFoot = xyz[11].Split(","[0]);
            string[] leftFoot = xyz[12].Split(","[0]);

            joints[int.Parse(shoulder[0])] = new Vector3(float.Parse(shoulder[1]), -float.Parse(shoulder[2]), -float.Parse(shoulder[3]));
            joints[int.Parse(upperArm[0])] = new Vector3(float.Parse(upperArm[1]), -float.Parse(upperArm[2]), -float.Parse(upperArm[3]));
            joints[int.Parse(leftShoulder[0])] = new Vector3(-float.Parse(leftShoulder[1]), float.Parse(leftShoulder[2]), float.Parse(leftShoulder[3]));
            joints[int.Parse(leftUpperArm[0])] = new Vector3(-float.Parse(leftUpperArm[1]), float.Parse(leftUpperArm[2]), float.Parse(leftUpperArm[3]));

            joints[int.Parse(rightUpperLeg[0])] = new Vector3(float.Parse(rightUpperLeg[1]), -float.Parse(rightUpperLeg[2]), -float.Parse(rightUpperLeg[3]));
            joints[int.Parse(leftUpperLeg[0])] = new Vector3(-float.Parse(leftUpperLeg[1]), float.Parse(leftUpperLeg[2]), float.Parse(leftUpperLeg[3]));
            joints[int.Parse(chest[0])] = new Vector3(float.Parse(chest[1]), 0, -float.Parse(chest[3]));
            joints[int.Parse(rightHand[0])] = new Vector3(float.Parse(rightHand[1]), -float.Parse(rightHand[2]), -float.Parse(rightHand[3]));
            joints[int.Parse(leftHand[0])] = new Vector3(-float.Parse(leftHand[1]), float.Parse(rightHand[2]), float.Parse(leftHand[3]));


            joints[int.Parse(rightLowerLeg[0])] = new Vector3(float.Parse(rightLowerLeg[1]), -float.Parse(rightLowerLeg[2]), -float.Parse(rightLowerLeg[3]) + 0.2f);
            joints[int.Parse(leftLowerLeg[0])] = new Vector3(-float.Parse(leftLowerLeg[1]), float.Parse(leftLowerLeg[2]), float.Parse(leftLowerLeg[3]) - 0.2f);

            joints[int.Parse(rightFoot[0])] = new Vector3(float.Parse(rightFoot[1]), -float.Parse(rightFoot[2]), -float.Parse(rightFoot[3]));
            joints[int.Parse(leftFoot[0])] = new Vector3(-float.Parse(leftFoot[1]), float.Parse(leftFoot[2]), float.Parse(leftFoot[3]));
        }
    }

    void ReceiveMessage()
    {
        while (true)
        {
            EndPoint serverPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7788);
            string message = "1";
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.SendTo(data, serverPoint);

            byte[] receivedMessage = new byte[1024];//用来接受数据
            int length = udpClient.ReceiveFrom(receivedMessage, ref serverPoint);//这个方法会把数据的来源（IP地址，端口号）放在第二个参数
            string outcome = Encoding.UTF8.GetString(receivedMessage, 0, length);
            //UpdatePosition(outcome);
            UpdateOrientation(outcome);
        }
    }

    private void OnDestroy()
    {
        udpClient.Close();
    }

    Quaternion XLookRotation(Vector3 right, Vector3 up)
    {
        Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(right, up);

        return forwardToTarget * rightToForward;
    }
}
