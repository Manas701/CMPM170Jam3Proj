using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowPlayers : MonoBehaviour
{
    
    public List<Transform> players;

    [Header("Position")]
    public Vector3 offset;
    public float smoothTime;
    private Vector3 centerPoint;
    private Vector3 newPos;

    [Header("Zoom")]
    public float minZoom;
    public float maxZoom;
    public float zoomLimiter;

    private Vector3 velocity;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (players.Count == 0)
            return;

        Move();
        Zoom();
    }

    void Move()
    {
        centerPoint = GetCenterPoint();

        newPos = centerPoint + offset;

        transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothTime);
    }

    void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime);
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 0; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }

        return bounds.size.x;
    }

    Vector3 GetCenterPoint()
    {
        if (players.Count == 1)
        {
            return players[0].position;
        }

        var bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 0; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }

        return bounds.center;
    }

}
