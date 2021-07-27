using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baxterHapticFeedback : MonoBehaviour
{
    // Start is called before the first frame update
    SensablePlugin sensablePlugin;
    private List<ContactPoint> contacts = new List<ContactPoint>();
    private float[] middleCollisionPoint;
    public float thresholdCollisionDetection = 0.3f;

    void Awake()
    {
        sensablePlugin = GetComponent<SensablePlugin>();
    }

    void Start()
    {

    }

    internal void OnCollisionEnterChild(Collision collision, GameObject gameObject)
    {
        float[] position = new float[3];
        collision.GetContacts(contacts);
        position = getNormal(contacts);
        
        if(middleCollisionPoint==null)
            sensablePlugin.recalculateJointAngles();

        if (gameObject!=sensablePlugin.collidingBaxterArticulation  || checkDistance(middleCollisionPoint, getMiddlePoint(contacts)))
            sensablePlugin.recalculateJointAngles();

        middleCollisionPoint = getMiddlePoint(contacts);

        //Debug.Log(position[0] + " " + 0);
        //Debug.Log(position[1] + " " + 1);
        //Debug.Log(position[2] + " " + 2);
        //position[0] *= 40;
        //position[1] *= 40;
        //position[2] *= 40;

        sensablePlugin.collidigObject = collision.collider.name;
        sensablePlugin.collidingBaxterArticulation = gameObject;
        sensablePlugin.isColliding = true;
        sensablePlugin.position = position;
    }

    internal void OnCollisionStayChild(Collision collision)
    {
        sensablePlugin.isColliding = true;
    }
    
    internal void OnCollisionExitChild(Collision collision)
    {
        float[] resetPos = { 0,0,0 };
        sensablePlugin.isColliding = false;
        sensablePlugin.position = resetPos;
    }

    public bool checkDistance(float[] point1, float[] point2)
    {
        Vector3 origin = new Vector3(point1[0], point1[1], point1[2]);
        Vector3 end = new Vector3(point2[0], point2[1], point2[2]);

        return Vector3.Distance(origin, end) > thresholdCollisionDetection ? true : false;
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
