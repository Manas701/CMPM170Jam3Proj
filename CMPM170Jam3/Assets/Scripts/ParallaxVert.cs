using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxVert : MonoBehaviour
{
    private float length;
    private float startPos;
    public GameObject cam;
    public float parallaxEffect;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float dist = ((cam.transform.position.x - 9) * parallaxEffect);

        transform.position = new Vector3(transform.position.x, startPos + dist, transform.position.z);
    }
}
