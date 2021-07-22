using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baxterHapticFeedback : MonoBehaviour
{
    // Start is called before the first frame update
    SensablePlugin sensablePlugin;
    private List<ContactPoint> contacts = new List<ContactPoint>();


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

        position[0] *= 40;
        position[1] *= 40;
        position[2] *= 40;

        sensablePlugin.recalculateJointAngles();
        sensablePlugin.collidigObject = collision.collider.name;
        sensablePlugin.isColliding = true;
        sensablePlugin.forces = position;

        Debug.Log("COLLISION: " + sensablePlugin.collidigObject);
    }

    internal void OnCollisionStayChild(Collision collision)
    {
        sensablePlugin.isColliding = true;
    }
    
    internal void OnCollisionExitChild(Collision collision)
    {
        sensablePlugin.isColliding = false;
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
