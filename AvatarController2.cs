using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarController2 : MonoBehaviour
{
    // Variable to hold all them bones. It will initialize the same size as initialRotations.
    protected Transform[] bones;
    private Animator animatorComponent = null;
    protected MotionInputManager miManager;
    public bool useUnscaledTime = false;

    // Start is called before the first frame update
    void Start()
    {
        miManager = MotionInputManager.Instance;
    }

    public void Awake()
    {
        // check for double start
        if (bones != null)
            return;
        if (!gameObject.activeInHierarchy)
            return;

        // inits the bones array
        bones = new Transform[25];
        // get the animator reference
        animatorComponent = GetComponent<Animator>();

        // Map bones to the points the Kinect tracks
        MapBones();
    }
    // Update is called once per frame
    void Update()
    {

        //bones[10].transform.up = new Vector3(-1, 1, -1); // this works by reverse direction of y axis.
        LookAtY(6);
        LookAtY(7);
        LookAtY(8);
        LookAtY(10);
        LookAtY(11);
        LookAtY(12);
        LookAtY(13);
        LookAtY(14);
        AlignWithX(15);
        LookAtY(17);
        LookAtY(18);
        AlignWithX(19);
        bones[1].transform.rotation = Quaternion.Slerp(bones[1].transform.rotation, Quaternion.LookRotation(miManager.GetJointPointing(2).normalized, Vector3.up), 10 * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));

        //AlignWithY(6);
        //AlignWithY(7);
        //AlignWithY(10);
        //AlignWithY(11);
        //AlignWithY(13);
        //AlignWithY(14);
        //AlignWithY(17);
        //AlignWithY(18);
        //AlignWithY(8);
        //AlignWithY(12);
        //AlignWithX(19);
        //AlignWithX(15);
        //miManager.UpdateOrientation();
        //bones[10].transform.rotation = Quaternion.LookRotation(new Vector3(1,0,0));//z,y
        //bones[10].transform.rotation = miManager.GetJointOrientation(6);

    }

    private void AlignWithY(int index)
    {
        Quaternion newRotation = Quaternion.FromToRotation(transform.up, miManager.GetJointPointing(index));
        bones[index].transform.rotation = Quaternion.Slerp(bones[index].transform.rotation, newRotation, 10 * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));
    }

    private void AlignWithX(int index)
    {
        Quaternion rightToForward = Quaternion.Euler(0f, -90f, 0f); ;
        Quaternion forwardToTarget;
        if (index == 19)
        {
            forwardToTarget = Quaternion.LookRotation(miManager.GetJointPointing(index), Vector3.up);
        }
        else
        {
            forwardToTarget = Quaternion.LookRotation(miManager.GetJointPointing(index), Vector3.down);
        }
        Quaternion newRotation = forwardToTarget * rightToForward;
        bones[index].transform.rotation = Quaternion.Slerp(bones[index].transform.rotation, newRotation, 10 * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));
    }

    private void LookAtY(int index)
    {
        Quaternion upToForward = Quaternion.Euler(-90f, 0f, 0f);
        Quaternion forwardToTarget;
        if (index == 6 | index == 7 | index == 8)
        {
            forwardToTarget = Quaternion.LookRotation(miManager.GetJointPointing(index), Vector3.up);
        }
        else if(index == 13 | index == 14 | index == 15|index == 16 | index == 17 | index == 18)
        {
            forwardToTarget = Quaternion.LookRotation(miManager.GetJointPointing(index), Vector3.left);
        }
        else
        {
            forwardToTarget = Quaternion.LookRotation(miManager.GetJointPointing(index), Vector3.down);
        }
        Quaternion newRotation = forwardToTarget * upToForward;
        bones[index].transform.rotation = Quaternion.Slerp(bones[index].transform.rotation, newRotation, 10 * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));
    }


    protected virtual void MapBones()
    {
        for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
        {
            if (!boneIndex2MecanimMap.ContainsKey(boneIndex))
                continue;

            bones[boneIndex] = animatorComponent ? animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]) : null;
        }
    }

    // dictionaries to speed up bone processing
    protected static readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
        {
            {0, HumanBodyBones.Hips},
            {1, HumanBodyBones.Spine},
            {2, HumanBodyBones.Chest},
            {3, HumanBodyBones.Neck},
            {4, HumanBodyBones.Head},

            {5, HumanBodyBones.LeftShoulder},
            {6, HumanBodyBones.LeftUpperArm},
            {7, HumanBodyBones.LeftLowerArm},
            {8, HumanBodyBones.LeftHand},

            {9, HumanBodyBones.RightShoulder},
            {10, HumanBodyBones.RightUpperArm},
            {11, HumanBodyBones.RightLowerArm},
            {12, HumanBodyBones.RightHand},

            {13, HumanBodyBones.LeftUpperLeg},
            {14, HumanBodyBones.LeftLowerLeg},
            {15, HumanBodyBones.LeftFoot},
            {16, HumanBodyBones.LeftToes},

            {17, HumanBodyBones.RightUpperLeg},
            {18, HumanBodyBones.RightLowerLeg},
            {19, HumanBodyBones.RightFoot},
            {20, HumanBodyBones.RightToes},

            {21, HumanBodyBones.LeftIndexProximal},
            {22, HumanBodyBones.LeftThumbProximal},
            {23, HumanBodyBones.RightIndexProximal},
            {24, HumanBodyBones.RightThumbProximal},
        };
}
