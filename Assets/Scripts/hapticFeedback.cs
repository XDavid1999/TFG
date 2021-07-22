using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
public class hapticFeedback : HapticClassScript
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

        HD_USER_STATUS_LIGHT = 0x2900
    }
    //Generic Haptic Functions
    private GenericFunctionsClass myGenericFunctionsClassScript;

    //Workspace Update Value
    float[] workspaceUpdateValue = new float[1];
    private int hapticDevice=0;
    [DllImport("hl")] public static extern int hlGetCurrentContext();
    [DllImport("hl")] public static extern void hlMakeCurrent(int hHD);
    [DllImport("hl")] public static extern int hlCreateContext(int hHD);
    [DllImport("hl")] public static extern void hlDeleteContext(int hHLRC);
    [DllImport("hd")] public static extern void hdStartScheduler();
    [DllImport("hd")] public static extern int hdInitDevice(char pConfigName);
    [DllImport("hd")] public static extern int hdGetCurrentDevice();
    [DllImport("hd")] public static extern void hdGetFloatv(HDenum pname, float[] values);
    [DllImport("hd")] public static extern void hdBeginFrame(int hHD);
    [DllImport("hd")] public static extern void hdEndFrame(int hHD);
    void Awake(){
        myGenericFunctionsClassScript = transform.GetComponent<GenericFunctionsClass>();
    }    
    // Start is called before the first frame update
    void Start()
    {
        try
        {

            PluginImport.InitHapticDevice();
            //hapticDevice = hdInitDevice((char)HDenum.HD_DEFAULT_DEVICE);
            //Debug.Log(hapticDevice);
            //int context= hlCreateContext(hapticDevice);
            //Debug.Log(context);
            //hlMakeCurrent(context);
            //Debug.Log("current");
            //hdStartScheduler();
            //Debug.Log("Success! Haptic Device Launched");

            //Update the Workspace as function of camera
            for (int i = 0; i < workspaceUpdateValue.Length; i++)
                workspaceUpdateValue[i] = myHapticCamera.transform.rotation.eulerAngles.y;

    //        PluginImport.UpdateHapticWorkspace(ConverterClass.ConvertFloatArrayToIntPtr(workspaceUpdateValue));

    //        //Set Mode of Interaction
    //        /*
			 //* Mode = 0 Contact
			 //* Mode = 1 Manipulation - So objects will have a mass when handling them
			 //* Mode = 2 Custom Effect - So the haptic device simulate vibration and tangential forces as power tools
			 //* Mode = 3 Puncture - So the haptic device is a needle that puncture inside a geometry
			 //*/
    //        PluginImport.SetMode(ModeIndex);
    //        //Show a text descrition of the mode
    //        myGenericFunctionsClassScript.IndicateMode();

    //        /***************************************************************/
    //        //Setup the Haptic Geometry in the OpenGL context 
    //        //And read haptic characteristics
    //        /***************************************************************/
    //        myGenericFunctionsClassScript.SetHapticGeometry();

    //        //Get the Number of Haptic Object
    //        Debug.Log("Haptic Objects: " + PluginImport.GetHapticObjectCount());

    //        /***************************************************************/
    //        //Launch the Haptic Event for all different haptic objects
    //        /***************************************************************/
    //        PluginImport.LaunchHapticEvent();
        }
        catch { Debug.Log("Task Failed: Enviroment Initialization"); }
    }

    // Update is called once per frame
    void Update()
    {
        //Update the Workspace as function of camera
        for (int i = 0; i < workspaceUpdateValue.Length; i++)
            workspaceUpdateValue[i] = myHapticCamera.transform.rotation.eulerAngles.y;

        PluginImport.UpdateHapticWorkspace(ConverterClass.ConvertFloatArrayToIntPtr(workspaceUpdateValue));
        
        /***************************************************************/
        //Update cube workspace
        /***************************************************************/
        myGenericFunctionsClassScript.UpdateGraphicalWorkspace();

        /***************************************************************/
        //Haptic Rendering Loop
        /***************************************************************/
        PluginImport.RenderHaptic();
        //Associate the cursor object with the haptic proxy value  
        myGenericFunctionsClassScript.GetProxyValues();

        Debug.Log("HL: "+ hlGetCurrentContext());

        myGenericFunctionsClassScript.GetTouchedObject();
    }

    void OnDisable()
    {
        try
        {
            PluginImport.HapticCleanUp();
        }
        catch { Debug.Log("Device's Turn Off Failed"); }
    }
}
