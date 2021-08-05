using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baxterHapticFeedback : MonoBehaviour
{
    // Start is called before the first frame update
    SensablePlugin sensablePlugin;
    mapBaxterArticulations mapBaxterArticulations;
    childCollider childCollider;
    private List<ContactPoint> contacts = new List<ContactPoint>();
    private float[] middleCollisionPoint;
    public float thresholdCollisionDetection = 0.5f;


    void Awake()
    {
        sensablePlugin = GetComponent<SensablePlugin>();
        mapBaxterArticulations = GetComponent<mapBaxterArticulations>();
        childCollider = GetComponent<childCollider>();
    }

    void Start()
    {

    }

    internal void OnCollisionEnterChild(Collision collision, GameObject gameObject)
    {
        sensablePlugin.getButtonStateSync();
        if (sensablePlugin.buttonActive && collision.gameObject.tag=="Grabbable" && !sensablePlugin.grabbingObject)
            grabbingObject(collision.gameObject, gameObject);
        else
        {
            float[] position = new float[3];
            collision.GetContacts(contacts);
            position = getNormal(contacts);

            if (middleCollisionPoint == null)
            {
                middleCollisionPoint = getMiddlePoint(contacts);
                sensablePlugin.recalculateSync();
            }

            if (gameObject!=sensablePlugin.collidingBaxterArticulation  || checkDistance(middleCollisionPoint, getMiddlePoint(contacts)))
            {
                if (sensablePlugin.positions.Count == sensablePlugin.positionsLength)
                    sensablePlugin.positions.Remove(sensablePlugin.positions[0]);

                sensablePlugin.positions.Add(position);
                sensablePlugin.recalculateSync();
            }

            middleCollisionPoint = getMiddlePoint(contacts);
            sensablePlugin.collidingBaxterArticulation = gameObject;
            sensablePlugin.isColliding = true;
        }
    }

    public void grabbingObject(GameObject collidingObject, GameObject baxterArticulation)
    {
        if(baxterArticulation == mapBaxterArticulations.selectedArticulations[mapBaxterArticulations.selectedArticulations.Length - 1].gameObject)
        {
            sensablePlugin.grabbingObjectGameobject = collidingObject.gameObject;
            sensablePlugin.isColliding = false;
            sensablePlugin.grabbingObject = true;
            collidingObject.transform.SetParent(baxterArticulation.transform);
            collidingObject.layer = 9;
            collidingObject.AddComponent<childCollider>();
            sensablePlugin.collidedRigidBodymass = sensablePlugin.grabbingObjectGameobject.GetComponent<Rigidbody>().mass;
            Destroy(sensablePlugin.grabbingObjectGameobject.GetComponent<Rigidbody>());
            unityResetArticulationAddingBugFix(true);
        }
    }

    internal void OnCollisionStayChild(Collision collision, GameObject gameObject)
    {
        sensablePlugin.getButtonStateSync();

        if (sensablePlugin.buttonActive && collision.gameObject.tag == "Grabbable" && !sensablePlugin.grabbingObject)
            grabbingObject(collision.gameObject, gameObject);
        else
            sensablePlugin.isColliding = true;
    }
    
    internal void OnCollisionExitChild(Collision collision)
    {
        middleCollisionPoint = null;
        sensablePlugin.isColliding = false;
        sensablePlugin.lastCollisionForceValues.Clear();
    }
    public void unityResetArticulationAddingBugFix( bool action)
    {
        List<Vector3> velocities = new List<Vector3>();
        List<ArticulationReducedSpace> positions = new List<ArticulationReducedSpace>();

        foreach (ArticulationBody articulation in mapBaxterArticulations.selectedArticulations)
        {
            velocities.Add(articulation.velocity);
            positions.Add(articulation.jointPosition);
        }

        if(action)
            sensablePlugin.grabbingObjectGameobject.AddComponent<ArticulationBody>();
        else
            Destroy(sensablePlugin.grabbingObjectGameobject.GetComponent<ArticulationBody>());

        for (int i = 0; i < mapBaxterArticulations.selectedArticulations.Length; i++)
        {
            mapBaxterArticulations.selectedArticulations[i].velocity = velocities[i];
            mapBaxterArticulations.selectedArticulations[i].jointPosition = positions[i];
        }
    }
    public bool checkDistance(float[] point1, float[] point2)
    {
        Vector3 origin = new Vector3(point1[0], point1[1], point1[2]);
        Vector3 end = new Vector3(point2[0], point2[1], point2[2]);

        return Mathf.Abs(Vector3.Distance(origin, end)) > thresholdCollisionDetection ? true : false;
    }
    public float[] getNormal(List<ContactPoint> contacts)
    {
        float[] normal = new float[3];

        foreach (ContactPoint contact in contacts)
        {
            normal[0] += contact.normal[0];
            normal[1] += contact.normal[1];
            normal[2] += contact.normal[2];
        }
        normal[0] /= contacts.Count;
        normal[1] /= contacts.Count;
        normal[2] /= contacts.Count;

        return normal;
    }

    public float[] getMiddlePoint(List<ContactPoint> contacts)
    {
        float[] position = new float[3];

        foreach (ContactPoint contact in contacts)
        {
            position[0] += contact.point.x;
            position[1] += contact.point.y;
            position[2] += contact.point.z;
        }

        position[0] /= contacts.Count;
        position[1] /= contacts.Count;
        position[2] /= contacts.Count;

        return position;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
