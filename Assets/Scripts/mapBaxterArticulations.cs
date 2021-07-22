using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class mapBaxterArticulations : MonoBehaviour
{
    // All Robot's Articulations
    private ArticulationBody[] articulationChain;
    //// Container List of Targets
    private List<float> targets = new List<float>();
    //// Robot's right Arm
    private ArticulationBody[] selectedArticulations;


    //// Necessary constants for the linear relation between haptic device's articulations and baxter
    private float[] aConstant = new float[6];
    private float[] bConstant = new float[6];

    private SensablePlugin sensablePlugin;

    private void Awake()
    {
        sensablePlugin = GetComponent<SensablePlugin>();
        articulationChain = this.GetComponentsInChildren<ArticulationBody>();
        selectedArticulations = getChildrenComponentsByName("right_arm_mount", articulationChain);
        string[] toRemove = { "right_arm_mount", "right_upper_elbow" };
        articulationChain = orderByTarget(articulationChain);
        selectedArticulations = removeByName(toRemove, selectedArticulations);
        AssignConstants();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    private ArticulationBody[] orderByTarget(ArticulationBody [] articulations)
    {
        ArticulationBody [] auxArticulationChain = new ArticulationBody [articulations.Length];
        int noTarget = 0;
        selectedArticulations[0].GetDriveTargets(targets);

        for (int i = 1; i < targets.Count + 1; ++i)
        {
            targets[i - 1] = (float)i;
        }

        selectedArticulations[0].SetDriveTargets(targets);

        for (int i = 0; i < articulations.Length; ++i)
        {
            
            if (articulations[i].xDrive.target !=0) {
                auxArticulationChain[(int)Mathf.Round((articulations[i].xDrive.target) * Mathf.Deg2Rad) - 1] = articulations[i];
            }
            else
            {
                auxArticulationChain[15 + noTarget] = auxArticulationChain[i];
                noTarget++;
            }
        }

        for (int i = 0; i < targets.Count; ++i)
        {
            targets[i] = 0;
        }

        selectedArticulations[0].SetDriveTargets(targets);

        return auxArticulationChain;
    }

    private int findSelectedIndex(ArticulationBody articulation)
    {
        for(int i=0; i < articulationChain.Length; ++i)
        {
            if (articulationChain[i] == articulation)
            {
                return i;
            }
        }
        return 0;
    }

    private ArticulationBody[] getChildrenComponentsByName(string name, ArticulationBody[] articulations)
    {
        for (int i = 0; i < articulations.Length; ++i)
        {
            if (articulations[i].name == name)
            {
                return articulations[i].GetComponentsInChildren<ArticulationBody>(); 
            }
        }

        return null;
    }

    private ArticulationBody[] removeByName(string[] names, ArticulationBody [] articulations)
    {
        int removed = 0;
        List<ArticulationBody> articulationList = articulations.ToList();

        for (int i = 0; i < articulationList.Count; ++i)
        {
            if (articulationList[i].name == names[removed])
            {
                articulationList.RemoveAt(i);
                removed++;
            }

            if (removed == names.Length)
                break;
        }

        return articulationList.ToArray();
    }

    private void AssignConstants()
    {
        aConstant[0] = (selectedArticulations[0].xDrive.upperLimit - selectedArticulations[0].xDrive.lowerLimit) / (SensablePlugin.S0_MaxAngle - SensablePlugin.S0_MinAngle);
        bConstant[0] = (selectedArticulations[0].xDrive.upperLimit - SensablePlugin.S0_MaxAngle * aConstant[0]) * Mathf.Deg2Rad;

        aConstant[1] = (selectedArticulations[1].xDrive.upperLimit - selectedArticulations[1].xDrive.lowerLimit) / (SensablePlugin.S1_MaxAngle - SensablePlugin.S1_MinAngle);
        bConstant[1] = (selectedArticulations[1].xDrive.upperLimit - SensablePlugin.S1_MaxAngle * aConstant[1]) * Mathf.Deg2Rad;

        aConstant[2] = (selectedArticulations[2].xDrive.upperLimit - selectedArticulations[2].xDrive.lowerLimit) / (SensablePlugin.E1_MaxAngle - SensablePlugin.E1_MinAngle);
        bConstant[2] = (selectedArticulations[2].xDrive.upperLimit - SensablePlugin.E1_MaxAngle * aConstant[2]) * Mathf.Deg2Rad;

        aConstant[3] = (selectedArticulations[3].xDrive.upperLimit - selectedArticulations[3].xDrive.lowerLimit) / (SensablePlugin.W0_MaxAngle - SensablePlugin.W0_MinAngle);
        bConstant[3] = (selectedArticulations[3].xDrive.upperLimit - SensablePlugin.W0_MaxAngle * aConstant[3]) * Mathf.Deg2Rad;

        aConstant[4] = (selectedArticulations[4].xDrive.upperLimit - selectedArticulations[4].xDrive.lowerLimit) / (SensablePlugin.W1_MaxAngle - SensablePlugin.W1_MinAngle);
        bConstant[4] = (selectedArticulations[4].xDrive.upperLimit - SensablePlugin.W1_MaxAngle * aConstant[4]) * Mathf.Deg2Rad;

        aConstant[5] = (selectedArticulations[5].xDrive.upperLimit - selectedArticulations[5].xDrive.lowerLimit) / (SensablePlugin.W2_MaxAngle - SensablePlugin.W2_MinAngle);
        bConstant[5] = (selectedArticulations[5].xDrive.upperLimit - SensablePlugin.W2_MaxAngle * aConstant[5]) * Mathf.Deg2Rad;
    }

    // Update is called once per frame
    void Update()
    {


    }

    private void LateUpdate()
    {
        for (int i = 0; i < sensablePlugin.JointAngles.Length; i++)
        {
            if (Mathf.Abs(targets[findSelectedIndex(selectedArticulations[i])] - sensablePlugin.JointAngles[i] * aConstant[i] + bConstant[i]) > 0.01)
                targets[findSelectedIndex(selectedArticulations[i])] = (targets[findSelectedIndex(selectedArticulations[i])] + sensablePlugin.JointAngles[i] * aConstant[i] + bConstant[i]) /2;
        }

        selectedArticulations[0].SetDriveTargets(targets);
    }
}
