using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformStick : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D other)
    {
        other.transform.parent = transform.parent;
    }

    void OnCollisionExit2D(Collision2D other)
    {
        other.transform.parent = null;
    }
}
