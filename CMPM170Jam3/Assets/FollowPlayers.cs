using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayers : MonoBehaviour
{
    private Transform p1;
    private Transform p2;
    private float camX;
    private float camY;

    // Start is called before the first frame update
    void Start()
    {
        p1 = GameObject.Find("Player1").transform;
        p2 = GameObject.Find("Player2").transform;
    }

    // Update is called once per frame
    void Update()
    {
        camX = (p1.position.x + p2.position.x) / 2;
        camY = (p1.position.y + p2.position.y) / 2;
        transform.position = new Vector3(camX, camY, transform.position.z);
    }
}
