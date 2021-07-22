using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
public class SensablePlugin : MonoBehaviour
{
    public enum HDenum
    {
        HD_DEFAULT_DEVICE = 0,

        HD_FORCE_OUTPUT = 0x4000,

        HD_CURRENT_BUTTONS = 0x2000,
        HD_CURRENT_SAFETY_SWITCH = 0x2001,
        HD_CURRENT_INKWELL_SWITCH = 0x2002,
        HD_CURRENT_ENCODER_VALUES = 0x2010,
        HD_CURRENT_PINCH_VALUE = 0x2011,
        HD_LAST_PINCH_VALUE = 0x2012,
        HD_CURRENT_POSITION = 0x2050,
        HD_CURRENT_VELOCITY = 0x2051,
        HD_CURRENT_TRANSFORM = 0x2052,
        HD_CURRENT_ANGULAR_VELOCITY = 0x2053,

        HD_CURRENT_JOINT_ANGLES = 0x2100,
        HD_CURRENT_GIMBAL_ANGLES = 0x2150,
        HD_LAST_BUTTONS = 0x2200,
        HD_LAST_SAFETY_SWITCH = 0x2201,
        HD_LAST_INKWELL_SWITCH = 0x2202,
        HD_LAST_ENCODER_VALUES = 0x2210,
        HD_LAST_POSITION = 0x2250,
        HD_LAST_VELOCITY = 0x2251,
        HD_LAST_TRANSFORM = 0x2252,
        HD_LAST_ANGULAR_VELOCITY = 0x2253,
        HD_LAST_JACOBIAN = 0x2254,
        HD_LAST_JOINT_ANGLES = 0x2300,
        HD_LAST_GIMBAL_ANGLES = 0x2350,

        HD_CURRENT_FORCE = 0x2700,
        HD_CURRENT_JOINT_TORQUE = 0x2703,
        HD_CURRENT_TORQUE = 0x2703,
        HD_CURRENT_GIMBAL_TORQUE = 0x2704,

        HD_NOMINAL_MAX_TORQUE_FORCE = 0x2622,
        HD_NOMINAL_MAX_TORQUE_CONTINUOUS_FORCE = 0x2623,
        HD_NOMINAL_MAX_FORCE               =0x2603,

        HD_CALLBACK_DONE = 0,
        HD_CALLBACK_CONTINUE = 1,

        HD_USER_STATUS_LIGHT = 0x2900
    }

    public enum HLenum
    {
        /* Force Effect Parameters */
        HL_EFFECT_CALLBACK,
        HL_EFFECT_CONSTANT,
        HL_EFFECT_SPRING,
        HL_EFFECT_VISCOUS,
        HL_EFFECT_FRICTION,

        HL_EFFECT_PROPERTY_TYPE,
        HL_EFFECT_PROPERTY_GAIN,
        HL_EFFECT_PROPERTY_MAGNITUDE,
        HL_EFFECT_PROPERTY_FREQUENCY,
        HL_EFFECT_PROPERTY_DURATION,
        HL_EFFECT_PROPERTY_POSITION,
        HL_EFFECT_PROPERTY_DIRECTION,
        HL_EFFECT_PROPERTY_ACTIVE,

        HL_EFFECT_COMPUTE_FORCE,
        HL_EFFECT_START,
        HL_EFFECT_STOP,

        /* Proxy and Device State 
           (in workspace coordinates) */
        HL_PROXY_POSITION,
        HL_PROXY_ROTATION,
        HL_PROXY_TRANSFORM,
        HL_DEVICE_POSITION,
        HL_DEVICE_ROTATION,
        HL_DEVICE_TRANSFORM,
        HL_DEVICE_FORCE,
        HL_DEVICE_TORQUE,
        HL_BUTTON1_STATE,
        HL_BUTTON2_STATE,
        HL_BUTTON3_STATE,
        HL_SAFETY_STATE,
        HL_INKWELL_STATE,
        HL_DEPTH_OF_PENETRATION,

        /* Proxy State */
        HL_PROXY_IS_TOUCHING,
        HL_PROXY_TOUCH_NORMAL,
    }

    public const float MIN_TORQUE = 40;
    public const float MAX_TORQUE = 900;

    private int hapticDevice;
    public bool isColliding = false;
    public string collidigObject;
    public float[] forces = new float[3];
    public float[] JointAngles = new float[6];
    float[] jointValues = new float[3];
    float[] gimbalValues = new float[3];

    float[] localJointValues = new float[3];
    float[] localGimbalValues = new float[3];
    float[] localJointAngles = new float[6];

    #region Device Angles
    public static readonly float S0_MinAngle = Mathf.Rad2Deg * -1f;
    public static readonly float S0_MaxAngle = Mathf.Rad2Deg * 1f;

    public static readonly float S1_MinAngle = Mathf.Rad2Deg * 0f;
    public static readonly float S1_MaxAngle = Mathf.Rad2Deg * 1.78f;

    public static readonly float E1_MinAngle = Mathf.Rad2Deg * -0.829f;
    public static readonly float E1_MaxAngle = Mathf.Rad2Deg * 1.2f;

    public static readonly float W0_MinAngle = Mathf.Rad2Deg * -2.5f;
    public static readonly float W0_MaxAngle = Mathf.Rad2Deg * 2.5f;

    public static readonly float W1_MinAngle = Mathf.Rad2Deg * -1.44f;
    public static readonly float W1_MaxAngle = Mathf.Rad2Deg * 1f;

    public static readonly float W2_MinAngle = Mathf.Rad2Deg * -2.61f;
    public static readonly float W2_MaxAngle = Mathf.Rad2Deg * 2.61f;
    #endregion

    #region Imported hd funcions
    [DllImport("hd")] public static extern int hdInitDevice(char pConfigName);
    [DllImport("hd")] public static extern void hdDisableDevice(char pConfigName);
    [DllImport("hd")] public static extern void hdStartScheduler();
    [DllImport("hd")] public static extern void hdStopScheduler();
    [DllImport("hd")] public static extern void hdBeginFrame(int hHD);
    [DllImport("hd")] public static extern void hdEndFrame(int hHD);
    [DllImport("hd")] public static extern void hdGetFloatv(HDenum pname, float[] values);
    [DllImport("hd")] public static extern void hdSetFloatv(HDenum pname, float[] values);
    [DllImport("hd")] public static extern void hdSetIntegerv(HDenum pname, int[] value);
    [DllImport("hd")] public static extern void hdGetIntegerv(HDenum pname, int[] value);
    [DllImport("hd")] public static extern void hdGetLongv(HDenum pname, long[] values);
    [DllImport("hd")] public static extern void hdGetDoublev(HDenum pname, double[] values);
    [DllImport("hd")] public static extern void hdGetBooleanv(HDenum pname, bool[] values);
    [DllImport("hd")] public static extern int hdGetCurrentDevice();
    [DllImport("hd")] public static extern void hdSetBooleanv(HDenum pname, bool[] values);
    [DllImport("hd")] public static extern void hdEnable(HDenum pname);
    [DllImport("hd")] public static extern void hdDisable(HDenum pname);
    [DllImport("hd")] public static extern bool hdIsEnabled(HDenum pnamevalues);
    [DllImport("hd")] public static extern void hdScheduleSynchronous(Func<int> function, object[] parameters, ushort priority);
    [DllImport("hd")] public static extern int hdScheduleAsynchronous(Func<int> function, object[] parameters, ushort priority);
    [DllImport("hd")] public static extern void hdSetSchedulerRate(long nRate);
    [DllImport("hd")] public static extern double hdGetSchedulerTimeStamp();
    #endregion

    #region Imported hl functions
    [DllImport("hl")] public static extern UIntPtr hlCreateContext(int hHD);
    [DllImport("hl")]  public static extern void hlDeleteContext(UIntPtr hHLRC);
    [DllImport("hl")]  public static extern void hlMakeCurrent(UIntPtr hHLRC);
    [DllImport("hl")]  public static extern void hlContextDevice(int hHD);
    [DllImport("hl")]  public static extern int hlGetCurrentContext();
    [DllImport("hl")]  public static extern int hlGetCurrentDevice();

    [DllImport("hl")] public static extern void hlBeginFrame();
    [DllImport("hl")] public static extern void hlEndFrame();
    [DllImport("hl")] public static extern void hlEnable(HLenum cap);
    [DllImport("hl")] public static extern void hlDisable(HLenum cap);
    [DllImport("hl")] public static extern bool hlIsEnabled(HLenum cap);

    [DllImport("hl")] public static extern void hlGetIntegerv(HLenum pname, int[] value);
    [DllImport("hl")] public static extern void hlGetDoublev(HLenum pname, double[] values);
    [DllImport("hl")] public static extern void hlGetBooleanv(HLenum pname, bool[] values);
    
    /* Force effects */
    [DllImport("hl")] public static extern void hlStartEffect(HLenum type, int effect); 
    [DllImport("hl")] public static extern void hlStopEffect(int effect); 
    [DllImport("hl")] public static extern void hlUpdateEffect(int effect);
    
    [DllImport("hl")] public static extern int hlGenEffects(double range);
    [DllImport("hl")] public static extern void hlDeleteEffects(int effect, double range);
    [DllImport("hl")] public static extern void hlIsEffect(int effect);
    [DllImport("hl")] public static extern void hlTriggerEffect(HLenum type);
    [DllImport("hl")] public static extern void hlEffectd(HLenum pname, double param);
    [DllImport("hl")] public static extern void hlEffecti(HLenum pname, int param);
    [DllImport("hl")] public static extern void hlEffectdv(HLenum pname, double[] parameters);
    [DllImport("hl")] public static extern void hlEffectiv(HLenum pname, int[] parameters);
    [DllImport("hl")] public static extern void hlGetEffectdv(int effect, HLenum pname, double[] parameters);
    [DllImport("hl")] public static extern void hlGetEffectiv(int effect, HLenum pname, int[] parameters);
    [DllImport("hl")] public static extern void hlGetEffectbv(int effect, HLenum pname, bool[] param);
    [DllImport("hl")] public static extern Exception hlGetError();

    [SerializeField] private Vector3 forceInput;
    #endregion
    private void Awake()
    {

    }
    private void Start()
    {
        HDenum[] capabilities = { HDenum.HD_FORCE_OUTPUT };
        hapticDevice = initDevice(capabilities);
    }


    private void Update()
    {
        hdBeginFrame(hapticDevice);
        CalculateJointAngles();
        SetForces();
        hdEndFrame(hapticDevice);

    }

    private int initDevice(HDenum[] capabilities)
    {
        int hapticDevice = hdInitDevice((char)HDenum.HD_DEFAULT_DEVICE);
        hdStartScheduler();
        //hdSetSchedulerRate(500);

        foreach (HDenum capability in capabilities)
        {
            hdEnable(capability);
        }

        return hapticDevice;
    }

    public int CalculateJointAngles()
    {
        hdGetFloatv(HDenum.HD_CURRENT_JOINT_ANGLES, jointValues);
        hdGetFloatv(HDenum.HD_CURRENT_GIMBAL_ANGLES, gimbalValues);

        for (int i = 0; i < 2; i++)
        {
            JointAngles[i] = jointValues[i];
        }
            JointAngles[2] = (jointValues[2] - jointValues[1]);

        for (int i = 0; i < 3; i++)
        {
            JointAngles[i + 3] = gimbalValues[i];
        }

        return (int)HDenum.HD_CALLBACK_CONTINUE;
    }

    public void recalculateJointAngles()
    {
        hdBeginFrame(hapticDevice);
        hdGetFloatv(HDenum.HD_CURRENT_JOINT_ANGLES, localJointValues);
        hdGetFloatv(HDenum.HD_CURRENT_GIMBAL_ANGLES, localGimbalValues);
        hdEndFrame(hapticDevice);

        for (int i = 0; i < 2; i++)
        {
            localJointAngles[i] = jointValues[i];
        }
        localJointAngles[2] = (jointValues[2] - jointValues[1]);

        for (int i = 0; i < 3; i++)
        {
            localJointAngles[i + 3] = gimbalValues[i];
        }
    }
    public void setVariableForce(){
        float[] baseForce = { MIN_TORQUE, MIN_TORQUE, MIN_TORQUE };


        
        hdSetFloatv(HDenum.HD_CURRENT_TORQUE, forces);
    }

    public float[] getDirectionVector(int index) {
        float[] solution = { 0f, 0f, 0f };
        float value = 0;

        for (int i = 0; i < index + 1; ++i)
        {
            value = localJointAngles[i] - JointAngles[i];

            switch (getArticulationAxis(i))
            {
                case "x":
                    solution[0] += value;
                    break;
                case "y":
                    solution[1] += value;
                    break;                
                case "z":
                    solution[2] += value;
                    break;
            }
        }

        return solution;
    }
    public string getArticulationAxis(int index)
    {
        switch (index) {
            case 0:
                return "y";
            case 1:
                return "x";
            case 2:
                return "x";
            case 3:
                return "z";
            case 4:
                return "x";
            case 5:
                return "z";
        }

        return "";
    }

    public void resetForce()
    {
        forces[0] = 0;
        forces[1] = 0;
        forces[2] = 0;

        hdSetFloatv(HDenum.HD_CURRENT_TORQUE, forces);
    }
    public void SetForces()
    {
        if (isColliding)
        {
            setVariableForce();
            //hdSetFloatv(HDenum.HD_CURRENT_TORQUE, forces);
        }
        else
        {
            resetForce();
        }
    }
    //public void hlForces()
    //{
    //    double[] direction = new double[3];
    //    //direction[0] = forces[0];
    //    //direction[1] = forces[1];
    //    //direction[2] = forces[2];
    //    direction[0] = 3;
    //    direction[1] = 0;
    //    direction[2] = 0;

    //    hlBeginFrame();
    //    hlEffectdv(HLenum.HL_EFFECT_PROPERTY_DIRECTION, direction);
    //    hlEffectd(HLenum.HL_EFFECT_PROPERTY_GAIN, 0.4);
    //    hlEffectd(HLenum.HL_EFFECT_PROPERTY_MAGNITUDE, 0.4);
    //    hlEffectd(HLenum.HL_EFFECT_PROPERTY_DURATION, 1000);
    //    hlTriggerEffect(HLenum.HL_EFFECT_CONSTANT);
    //    hlEndFrame();
    //}

    private void OnApplicationQuit()
    {
        //hlMakeCurrent(-1);
        //hlDeleteContext(hHLRC);
        hdStopScheduler();
        hdDisableDevice((char)hapticDevice);
    }

}
