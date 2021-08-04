using System;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

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
        HD_NOMINAL_MAX_FORCE = 0x2603,

        HD_DEVICE_BUTTON_1 = (1 << 0),      
        HD_DEVICE_BUTTON_2 = (1 << 1),      
        HD_DEVICE_BUTTON_3 = (1 << 2),      
        HD_DEVICE_BUTTON_4 = (1 << 3),      

        HD_CALLBACK_DONE = 0,
        HD_CALLBACK_CONTINUE = 1,

        HD_INSTANTANEOUS_UPDATE_RATE = 0x2601,
        HD_USER_STATUS_LIGHT = 0x2900
    }


    mapBaxterArticulations mapBaxterArticulations;
    /* Minimum/Maximum value of torque we will use */
    public const float MIN_TORQUE = 40;
    public const float MAX_TORQUE = 1200;
    public float GRABBING_CORRECTOR = 0.5f;
    public static float[] MAX_PENETRATIONS = { 0.3f, 0.2f, 0.3f };

    float[] CONSTANTS = { Mathf.Log(MAX_TORQUE - MIN_TORQUE) / MAX_PENETRATIONS[0], Mathf.Log(MAX_TORQUE - MIN_TORQUE) / MAX_PENETRATIONS[1], Mathf.Log(MAX_TORQUE - MIN_TORQUE) / MAX_PENETRATIONS[2] };
    /* ID of the initialized device */
    private int hapticDevice;
    /* Is the robot colliding? */
    public bool isColliding = false;
    public bool buttonActive = false;
    public bool grabbingObject = false;
    /* Name of the object we are colliding with */
    public string collidigObject;
    /* Name of the object we are colliding with */
    public GameObject collidingBaxterArticulation;
    public GameObject grabbingObjectGameobject = null;
    /* Current forces set in haptic device */
    public float[] forces = new float[3];
    /* Position and velocities when grabbing an object */
    public Vector3 lastGameobjectPosition = new Vector3();
    public float[] currentVelocity = { 0, 0, 0 };
    public float[] lastVelocity = { 0, 0, 0 };
    /* Collision's direction */
    public List<float[]> positions = new List<float[]>();
    public int positionsLength = 2;
    /* Current Angles in haptic device */
    public float[] JointAngles = new float[6];
    float[] jointValues = new float[3];
    float[] gimbalValues = new float[3];
    /* Angles of haptic device when the collision started */
    float[] localJointValues = new float[3];
    float[] localGimbalValues = new float[3];
    float[] localJointAngles = new float[6];

    public List<string> infoX = new List<string>();
    public List<string> infoY = new List<string>();
    public List<string> infoZ = new List<string>();

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
    [DllImport("hd")] public static extern ulong hdScheduleSynchronous(Func<int> function, object[] parameters, ushort priority);
    [DllImport("hd")] public static extern int hdScheduleAsynchronous(Func<int> function, object[] parameters, ushort priority);
    [DllImport("hd")] public static extern void hdSetSchedulerRate(long nRate);
    [DllImport("hd")] public static extern void hdUnschedule(ulong handler);
    [DllImport("hd")] public static extern double hdGetSchedulerTimeStamp();
    #endregion

 
    private void Awake()
    {
        mapBaxterArticulations = GetComponent<mapBaxterArticulations>();
        HDenum[] capabilities = { HDenum.HD_FORCE_OUTPUT };
        hapticDevice = initDevice(capabilities);
    }

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        isGrabbingObject();
        isTouchingObject();
        hdScheduleSynchronous(updateHapticContext, null, ushort.MaxValue);
    }
    private void OnApplicationQuit()
    {
        writeInFile(infoX, "infoX");
        writeInFile(infoY, "infoY");
        writeInFile(infoZ, "infoZ");
        disableDevice();
    }
    public void isGrabbingObject()
    {
        if (grabbingObject)
        {
            getButtonStateSync();
            if (buttonActive) {
                float mass = grabbingObjectGameobject.GetComponent<Rigidbody>().mass;
                Vector3 position = grabbingObjectGameobject.transform.position;

                resetForce();
                setVelocity(position);

                Vector3 calculatedInertia = inertia(mass);
                Vector3 calculatedGravity = gravity(mass);

                for (int i = 0; i < forces.Length; ++i)
                {
                    float grabbingForce = calculatedInertia[i] + calculatedGravity[i];

                    if (Math.Abs(grabbingForce) > MAX_TORQUE)
                        forces[i] = Mathf.Sign(grabbingForce) * MAX_TORQUE;
                    else
                        forces[i] = grabbingForce;                  
                }

                updatePositionAndVelocity(position);
                infoY.Add(forces[0].ToString());
                infoX.Add(forces[1].ToString());
                infoZ.Add(forces[2].ToString());
            }
            else
            {
                lastGameobjectPosition = Vector3.zero;
                grabbingObject = false;
                Destroy(grabbingObjectGameobject.GetComponent<childCollider>());
                grabbingObjectGameobject.layer = 0;
                grabbingObjectGameobject.transform.SetParent(null);
                resetForce();
            }
        }
    }

    public void updatePositionAndVelocity(Vector3 position)
    {
        lastGameobjectPosition = position;
        lastVelocity[0] = currentVelocity[0];
        lastVelocity[1] = currentVelocity[1];
        lastVelocity[2] = currentVelocity[2];
    }
    public Vector3 fromFloatArrToVector3(float[] array)
    {
        if (array.Length != 3)
            return new Vector3(array[0], array[1], array[2]);

        return Vector3.zero;
    }

    public void deleteData()
    {
        writeInFile(infoX, "infoX");
        writeInFile(infoY, "infoY");
        writeInFile(infoZ, "infoZ");

        infoY.Clear();
        infoX.Clear();
        infoZ.Clear();
    }
    public void setVelocity(Vector3 newPosition)
    {
        Vector3 velocity = getVelocityFromPosition(newPosition);

        currentVelocity[0] = velocity[0]; 
        currentVelocity[1] = velocity[1]; 
        currentVelocity[2] = velocity[2]; 
    }

    public Vector3 getVelocityFromPosition(Vector3 newPosition)
    {
        Vector3 solution = Vector3.zero;
        float MIN_VARIATION = 0.0055f;

        if (lastGameobjectPosition != Vector3.zero)
        {
            if (Vector3.Distance(lastGameobjectPosition, newPosition) > MIN_VARIATION)
            {
                solution[0] = (lastGameobjectPosition[0] - newPosition[0]) / Time.deltaTime;
                solution[1] = (lastGameobjectPosition[1] - newPosition[1]) / Time.deltaTime;
                solution[2] = (lastGameobjectPosition[2] - newPosition[2]) / Time.deltaTime;
            }
            else
            {
                solution[0] = 0.5f * (lastGameobjectPosition[0] - newPosition[0]) / Time.deltaTime;
                solution[1] = 0.5f * (lastGameobjectPosition[1] - newPosition[1]) / Time.deltaTime;
                solution[2] = 0.5f * (lastGameobjectPosition[2] - newPosition[2]) / Time.deltaTime;
            }
        }

        return solution;
    }
    public Vector3 gravity(float mass)
    {
        Vector3 gravity = new Vector3();
        float[] weight = { 0, 0.7f, 0.3f };
        const float GRAVITY_SCALE = 0.3f;

        gravity[0] = weight[0] * GRAVITY_SCALE * mass * Physics.gravity[0];
        gravity[1] = weight[1] * GRAVITY_SCALE * mass * Physics.gravity[1];
        gravity[2] = weight[2] * GRAVITY_SCALE * mass * Physics.gravity[1];

        return gravity;
    }
    public Vector3 inertia(float mass)
    {
        float FLIP_TORQUE_SENSE = -1f;
        float INERTIA_SCALE = -0.15f;
        float DISTANCE_TO_ORIGIN = -1.1f;

        float acceleration;
        float force;
        float torque;
        Vector3 inertia = new Vector3();

        for (int i = 0; i < forces.Length; ++i)
        {
            acceleration = getAcceleration(i, mass);
            force = acceleration * mass;
            torque = force * DISTANCE_TO_ORIGIN;

            inertia[i] = FLIP_TORQUE_SENSE * INERTIA_SCALE * torque;
        }

        return inertia;
    }

    public float getAcceleration(int i, float mass)
    {
        float MIN_ACCELERATION = 0.001f;
        float MAX_ACCELERATION = MAX_TORQUE/mass;

        float acceleration = (lastVelocity[i] - currentVelocity[i]) / Time.deltaTime;

        return Mathf.Abs(acceleration) > MAX_ACCELERATION ? 0 : Mathf.Abs(acceleration) > MIN_ACCELERATION ? acceleration : 0f;

    }

    /** Función para inicializar el dispositivo háptico */
    private int initDevice(HDenum[] capabilities)
    {
        int hapticDevice = hdInitDevice((char)HDenum.HD_DEFAULT_DEVICE);
        hdStartScheduler();

        foreach (HDenum capability in capabilities)
        {
            hdEnable(capability);
        }

        return hapticDevice;
    }
    public void recalculateSync()
    {
        hdScheduleSynchronous(recalculateJointAngles, null, ushort.MaxValue);
    }
    /** Función que devuelve un mapeo de los ángulos del dispositivo háptico en los vectores
     * utilizados */
    public int updateHapticContext()
    {
        hdBeginFrame(hapticDevice);        
        hdGetFloatv(HDenum.HD_CURRENT_JOINT_ANGLES, jointValues);
        hdGetFloatv(HDenum.HD_CURRENT_GIMBAL_ANGLES, gimbalValues);
        hdSetFloatv(HDenum.HD_CURRENT_TORQUE, forces);

        for (int i = 0; i < 2; i++)
        {
            JointAngles[i] = jointValues[i];
        }
        JointAngles[2] = (jointValues[2] - jointValues[1]);

        for (int i = 0; i < 3; i++)
        {
            JointAngles[i + 3] = gimbalValues[i];
        }
        hdEndFrame(hapticDevice);

        return (int)HDenum.HD_CALLBACK_DONE;
    }

    public void getButtonStateSync()
    {
        hdScheduleSynchronous(getButtonState, null, ushort.MaxValue);
    }

    public int getButtonState()
    {
        int [] buttons = new int[1];
        hdBeginFrame(hapticDevice);
        hdGetIntegerv(HDenum.HD_CURRENT_BUTTONS, buttons);

        if (buttons[0] == 0)
            buttonActive = false;
        else
            buttonActive = true;

        hdEndFrame(hapticDevice);

        return (int)HDenum.HD_CALLBACK_DONE;
    }

    /** Función para guardar los ángulos de baxter en el momento en que colisiona */
    public int recalculateJointAngles()
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

        return (int)HDenum.HD_CALLBACK_DONE;
    }
    /** Actualiza el vector que contiene las últimas fuerzas seteadas */

    public float[] getLastPos()
    {
        float[] sol = { 0, 0, 0 };

        foreach (float[] pos in positions)
        {
            sol[0] += pos[0];
            sol[1] += pos[1];
            sol[2] += pos[2];
        }

        sol[0] /= positions.Count;
        sol[1] /= positions.Count;
        sol[2] /= positions.Count;

        return sol;
    }
    /** La fuerza variable se seteará con una función exponencial: 
     * f(x) = MIN_TORQUE * (e^(log((MAX_TORQUE - MIN_TORQUE)/MAX_PENETRATION)* variación))
     * 
    * La razón de usar esta función es sencilla: queremos un feedback suave al tocar y alto cuando tratemos 
    * de penetrar un objeto. Trataremos de suavizar el cambio de fuerza comparando los valores actuales de 
    * fuerza con los anteriores, haciendo así transiciones más suaves */
    public void setVariableForce() 
    {
        int index = collidingArticulationIndex();
        float[] variation = getDirectionVector(index);
        float[] position = getLastPos();
        float calculatedForce, difference;
        int sense;

        for (int i = 0; i < forces.Length; i++)
        {
            if (positions[positions.Count - 1][i] == 0f)
                sense = (int)Mathf.Sign(position[i]);
            else
                sense = (int)Mathf.Sign(positions[positions.Count - 1][i]);

            if(isExiting(sense, variation[i], i))
            {
                forces[i] = 0;
            }
            else
            {
                calculatedForce = sense * (MIN_TORQUE + Mathf.Exp(CONSTANTS[i] * Mathf.Abs(variation[i])));

                if (Mathf.Abs(variation[i]) < 0.02)
                {
                    if (i != 1)
                        forces[i] = 0;
                    else
                        forces[i] = MIN_TORQUE;
                }
                else if (Mathf.Abs(variation[i]) < 0.02 && Mathf.Abs(variation[i]) < 0.06)
                {
                    if (i != 1)
                        forces[i] = MIN_TORQUE;
                    else
                        forces[i] = MIN_TORQUE * 1.2f;
                }
                else if (Mathf.Abs(variation[i]) < 0.06 && Mathf.Abs(variation[i]) < 0.1)
                {
                    forces[i] = 0.7f * calculatedForce;
                }
                else
                {
                    calculatedForce = calculatedForce + forces[i] / 2;

                    if (calculatedForce > MAX_TORQUE)
                        forces[i] = sense * MAX_TORQUE;
                    else
                        forces[i] = calculatedForce;
                }

                difference = Mathf.Abs(calculatedForce) - Mathf.Abs(forces[i]);

                if (difference > MIN_TORQUE * 1.2)
                {
                    forces[i] += sense * MIN_TORQUE;
                }

            }
        }
    }

    public int collidingArticulationIndex()
    {
        for (int i = 0; i < mapBaxterArticulations.selectedArticulations.Length; i++)
        {
            if (mapBaxterArticulations.selectedArticulations[i] == collidingBaxterArticulation.GetComponent<ArticulationBody>())
                return i;
        }

        return -1;
    }

    /** Detecta si tras la colisión estamos tratando de ir más profundo o si estamos tratando de ir hacia afuera */
    public bool isExiting(int sense, float variation, int index)
    {
        switch (index)
        {
            case 0:
                if (sense > 0)
                    if (Mathf.Sign(variation) < 0)
                        return false;
                    else
                        return true;
                else
                    if (Mathf.Sign(variation) < 0)
                        return true;
                    else
                        return false;
            case 1:
                if (sense > 0)
                    if (Mathf.Sign(variation) < 0)
                        return true;
                    else
                        return false;
                else
                    if (Mathf.Sign(variation) < 0)
                        return false;
                    else
                        return true;

            case 2:
                if (sense > 0)
                    if (Mathf.Sign(variation) < 0)
                        return true;
                    else
                        return false;
                else
                    if (Mathf.Sign(variation) < 0)
                        return false;
                    else
                        return true;
        }

        return true;

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

                return value / range;
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

    /** Función que retorna un vector de dirección conocidos los ángulos de baxter,
     * usada para establecer feedback háptico*/
    public float[] getDirectionVector(int index) {
        float minZCollisionValue = 0.25f;
        float minYCollisionValue = 0.1f;
        float[] solution = { 0f, 0f, 0f };
        /** Importancia de la articulación iésima en el movimiento en su eje */
        float[] weightX = { 0f, 0.6f, 0.25f, 0f, 0.15f, 0f };
        float[] weightZ = { 0f, 0.2f, 0.6f, 0f, 0.2f, 0f };
        float value;
        
        for (int i = 0; i < index; ++i)
        {
            value = normalizeHapticAngles(i, localJointAngles[i]) - normalizeHapticAngles(i, JointAngles[i]);
            

            if (getArticulationAxis(i).Contains("x"))
                solution[1] += value * weightX[i];
            if (getArticulationAxis(i).Contains("y"))
                solution[0] += value;
            if (getArticulationAxis(i).Contains("z"))
                solution[2] += value * weightZ[i];
        }

        if (Mathf.Abs(solution[0]) < minYCollisionValue)
            solution[0] *= 0.1f;
        if (Mathf.Abs(solution[2]) < minZCollisionValue)
            solution[2] *= 0.5f;

        return solution;
    }

    public void writeInFile(List<string> toWrite, string name){
        try
        {
            //Pass the filepath and filename to the StreamWriter Constructor
            StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/" + name + ".txt");
            //Write a line of text
            foreach (string line in toWrite)
                sw.WriteLine(line);
            //Close the file
            sw.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
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
    }
    public void isTouchingObject()
    {
        if (isColliding)
        {
            setVariableForce();
        }
        else if(!grabbingObject && !isColliding)   
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
