using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformStick : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision other)
    {
        other.transform.parent = transform;
    }

    void OnCollisionExit(Collision other)
    {
        other.transform.parent = null;
    }
}
