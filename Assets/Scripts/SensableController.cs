using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Copies the joint positions from the sensable controller to the unity model
/// </summary>
public class SensableController : MonoBehaviour
{
    //[Tooltip("Articulation root of the arm that will be moved")]
    [SerializeField] private ArticulationBody[] articulations;

    //The mapping between the Sensable angles and the device angles will be in a linear form: D_Angle = S_angle * a + b
    //With the condition that both angles will reach their maximum and minimum togheter at the same point

    private float[] aConstant = new float[6];
    private float[] bConstant = new float[6];

    private List<float> driveTargets;

    private SensablePlugin sensablePlugin;
    
    private void Awake()
    {
        driveTargets = new List<float>();

        sensablePlugin = GetComponent<SensablePlugin>();
    }

    private void Start()
    {
        articulations[0].GetDriveTargets(driveTargets);

        AssignConstants();
    }


    private void AssignConstants()
    {
        aConstant[0] = (articulations[0].xDrive.upperLimit - articulations[0].xDrive.lowerLimit) / (SensablePlugin.S0_MaxAngle - SensablePlugin.S0_MinAngle);
        bConstant[0] = (articulations[0].xDrive.upperLimit - SensablePlugin.S0_MaxAngle * aConstant[0]) * Mathf.Deg2Rad;

        aConstant[1] = (articulations[1].xDrive.upperLimit - articulations[1].xDrive.lowerLimit) / (SensablePlugin.S1_MaxAngle - SensablePlugin.S1_MinAngle);
        bConstant[1] = (articulations[1].xDrive.upperLimit - SensablePlugin.S1_MaxAngle * aConstant[1]) * Mathf.Deg2Rad;

        aConstant[2] = (articulations[2].xDrive.upperLimit - articulations[2].xDrive.lowerLimit) / (SensablePlugin.E1_MaxAngle - SensablePlugin.E1_MinAngle);
        bConstant[2] = (articulations[2].xDrive.upperLimit - SensablePlugin.E1_MaxAngle * aConstant[2]) * Mathf.Deg2Rad;

        aConstant[3] = (articulations[3].xDrive.upperLimit - articulations[3].xDrive.lowerLimit) / (SensablePlugin.W0_MaxAngle - SensablePlugin.W0_MinAngle);
        bConstant[3] = (articulations[3].xDrive.upperLimit - SensablePlugin.W0_MaxAngle * aConstant[3]) * Mathf.Deg2Rad;

        aConstant[4] = (articulations[4].xDrive.upperLimit - articulations[4].xDrive.lowerLimit) / (SensablePlugin.W1_MaxAngle - SensablePlugin.W1_MinAngle);
        bConstant[4] = (articulations[4].xDrive.upperLimit - SensablePlugin.W1_MaxAngle * aConstant[4]) * Mathf.Deg2Rad;

        aConstant[5] = (articulations[5].xDrive.upperLimit - articulations[5].xDrive.lowerLimit) / (SensablePlugin.W2_MaxAngle - SensablePlugin.W2_MinAngle);
        bConstant[5] = (articulations[5].xDrive.upperLimit - SensablePlugin.W2_MaxAngle * aConstant[5]) * Mathf.Deg2Rad;
    }

    private void LateUpdate()
    {
        for (int i = 0; i < sensablePlugin.JointAngles.Length; i++)
        {
            driveTargets[i] = sensablePlugin.JointAngles[i] * aConstant[i] + bConstant[i];
        }

        articulations[0].SetDriveTargets(driveTargets);
    }
}