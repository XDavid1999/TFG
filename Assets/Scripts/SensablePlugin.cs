using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

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

    mapBaxterArticulations mapBaxterArticulations;
    /* Minimum/Maximum value of torque we will use */
    public const float MIN_TORQUE = 40;
    public const float MAX_TORQUE = 900;
    public static float[] MAX_PENETRATIONS = { 0.2f, 0.2f, 0.1f };

    float[] CONSTANTS = { Mathf.Log(MAX_TORQUE - MIN_TORQUE) / MAX_PENETRATIONS[0], Mathf.Log(MAX_TORQUE - MIN_TORQUE) / MAX_PENETRATIONS[1], Mathf.Log(MAX_TORQUE - MIN_TORQUE) / MAX_PENETRATIONS[2] };
    /* ID of the initialized device */
    private int hapticDevice;
    /* Is the robot colliding? */
    public bool isColliding = false;
    /* Name of the object we are colliding with */
    public string collidigObject;
    /* Name of the object we are colliding with */
    public GameObject collidingBaxterArticulation;
    /* Current forces set in haptic device */
    public float[] forces = new float[3];
    private List<float[]> lastForces = new List<float[]>(); 
    int lastForcesLength = 3;
    /* Collision's direction */
    public float[] position = new float[3];
    /* Current Angles in haptic device */
    public float[] JointAngles = new float[6];
    float[] jointValues = new float[3];
    float[] gimbalValues = new float[3];
    /* Angles of haptic device when the collision started */
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
        mapBaxterArticulations = GetComponent<mapBaxterArticulations>();
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
    private void OnApplicationQuit()
    {
        disableDevice();
    }
    /** Función para inicializar el dispositivo háptico */
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
    /** Función que devuelve un mapeo de los ángulos del dispositivo háptico en los vectores
     * utilizados */
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
    /** Función para guardar los ángulos de baxter en el momento en que colisiona */
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
    /** Actualiza el vector que contiene las últimas fuerzas seteadas */
    private void updateLastForces(float[] newforces)
    {
        if (lastForces.Count==lastForcesLength)
            lastForces.RemoveAt(0);

        for(int i = 0; i < newforces.Length; ++i)
            newforces[i] = MIN_TORQUE + Mathf.Exp(CONSTANTS[i] * newforces[i]);

        lastForces.Add(newforces);
    }
    /** Calcula la nueva fuerza a partir de las últimas fuerzas usadas en el dispositivo */
    private float[] getComparedForce(List<float[]> forces)
    {
        float[] newForce = new float[3];
        for (int i = 0; i < forces.Count; ++i)
        {
            newForce[0] += forces[i][0];
            newForce[1] += forces[i][1];
            newForce[2] += forces[i][2];
        }

        newForce[0] /= forces.Count;
        newForce[1] /= forces.Count;
        newForce[2] /= forces.Count;

        return newForce;
    }
    /** La fuerza variable se seteará con una función exponencial: f(x) = MIN_TORQUE * (e^(50*variación))
    * La razón de usar esta función es sencilla: queremos un feedback suave al tocar y alto cuando tratemos 
    * de penetrar un objeto. Trataremos de suavizar el cambio de fuerza comparando los valores actuales de 
    * fuerza con los anteriores, haciendo así transiciones más suaves */
    public void setVariableForce(){
        int index = getBaxterArticulationIndex(collidingBaxterArticulation);
        float[] variation = getDirectionVector(index);
        //updateLastForces(variation);
        //float[] lastForceSum = getComparedForce(lastForces);
        
        //constant = 10;
        float calculatedForce, difference;

        for (int i = 0; i < variation.Length; i++)
        { 
            calculatedForce = Mathf.Sign(position[i]) * (MIN_TORQUE + Mathf.Exp(CONSTANTS[i] * Mathf.Abs(variation[i])));
            
            difference = Mathf.Abs(calculatedForce) - Mathf.Abs(forces[i]);

            if (variation[i] < 0.05)
            {
                forces[i] = calculatedForce - MIN_TORQUE;
            }
            else
            {
                if (difference > 50)
                    forces[i] += 20;
                else
                {
                    if (calculatedForce > MAX_TORQUE)
                        forces[i] = MAX_TORQUE;
                    else
                        forces[i] = calculatedForce;
                }
            }
            Debug.Log(forces[i]);
        }
        hdSetFloatv(HDenum.HD_CURRENT_TORQUE, forces);
    }
    
    /** Devuelve un valor entre 0 y 1 del desplazamiento que el robot ha hecho en la articulación dada,
     es decir, devuelve de forma ponderada cual ha sido la cantidad, dentro del rango posible*/
   public float normalizeHapticAngles(int index, float value)
    {
        float min;
        float max;
        float range;
        value = Mathf.Abs(value);

        switch (index)
        {
            case 0:
                min = 0.913f;
                max = -0.913f;
                range = Mathf.Abs(min) + Mathf.Abs(max);

                return value/range;
            case 1:
                min = 0.330f;
                max = 1.726f;
                range = Mathf.Abs(min) + Mathf.Abs(max);

                return value / range;
            case 2:
                min = -1.007f;
                max = -0.402f;
                range = Mathf.Abs(min) + Mathf.Abs(max);

                return value / range;
            case 3:
                min = -2.509f;
                max = 2.296f;
                range = Mathf.Abs(min) + Mathf.Abs(max);

                return value / range;
            case 4:
                min = -1.426f;
                max = 1.057f;
                range = Mathf.Abs(min) + Mathf.Abs(max);

                return value / range;
            case 5:
                min = -2.872f;
                max = 2.628f;
                range = Mathf.Abs(min) + Mathf.Abs(max);

                return value / range;
        }

        return -1;
    }

    /** Función que retorna el índice correspondiente dada una articulación de baxter*/
    public int getBaxterArticulationIndex(GameObject baxterArticulation)
    {
        return Array.IndexOf(mapBaxterArticulations.selectedArticulations, baxterArticulation.GetComponent<ArticulationBody>());
    }

    /** Función que retorna un vector de dirección conocidos los ángulos de baxter,
     * usada para establecer feedback háptico*/
    public float[] getDirectionVector(int index) {
        float minZCollisionValue = 0.4f;
        float minYCollisionValue = 0.2f;
        float[] solution = { 0f, 0f, 0f };
        /** Importancia de la articulación iésima en el movimiento en su eje */
        float[] weightX = { 0f, 0.6f, 0.25f, 0f, 0.15f, 0f };
        float[] weightZ = { 0f, 0.2f, 0.6f, 0f, 0.2f, 0f };
        float value;
       
        for (int i = 0; i < index + 1; ++i)
        {
            value = normalizeHapticAngles(i, localJointAngles[i]) - normalizeHapticAngles(i, JointAngles[i]);

            if (getArticulationAxis(i).Contains("x"))
                solution[1] += value * weightX[i];
            if (getArticulationAxis(i).Contains("y"))
                if (value > minYCollisionValue)
                    solution[0] += value;
                else
                    solution[0] += value *0.01f;
            if (getArticulationAxis(i).Contains("z"))
                if (value > minZCollisionValue)
                    solution[2] += value * weightZ[i];
                else
                    solution[2] += value * weightZ[i] * 0.1f;
        }

        //Debug.Log(solution[0] + " " + 0);
        //Debug.Log(solution[1] + " " + 1);
        //Debug.Log(solution[2] + " " + 2);

        return solution;
    }

    /** Devuelve el eje de acción de cada articulación por orden */
    public string getArticulationAxis(int index)
    {
        switch (index) {
            case 0:
                return "y";
            case 1:
                return "xz";
            case 2:
                return "xz";
            case 3:
                return "";
            case 4:
                return "xz";
            case 5:
                return "";
        }

        return "";
    }
    /** Función para eliminar todas las fuerzas establecidas */
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
        }
        else
        {
            resetForce();
        }
    }
    public void disableDevice()
    {
        hdStopScheduler();
        hdDisableDevice((char)hapticDevice);
    }
}
