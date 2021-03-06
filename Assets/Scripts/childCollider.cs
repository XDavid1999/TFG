using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class childCollider : MonoBehaviour
{

    private baxterHapticFeedback script;
    // Start is called before the first frame 
    void Start()
    {
        script = GetComponentInParent<baxterHapticFeedback>();
    }

    void OnCollisionEnter(Collision collision)
    {
        script.OnCollisionEnterChild(collision, transform.parent.parent.parent.gameObject);
    }

    void OnCollisionStay(Collision collision)
    {
        script.OnCollisionStayChild(collision, transform.parent.parent.parent.gameObject);
    }

    void OnCollisionExit(Collision collision)
    {
        script.OnCollisionExitChild(collision);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
