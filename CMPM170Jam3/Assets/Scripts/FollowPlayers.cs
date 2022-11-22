using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowPlayers : MonoBehaviour
{
    
    public List<Transform> players;

    [Header("Position")]
    // how far the camera is from the centerpoint
    public Vector3 offset;

    // time taken to move the camera to the new position
    public float smoothTime;

    // center point between both players
    private Vector3 centerPoint;

    private Vector3 newPos;

    [Header("Zoom")]
    public float minZoom;
    public float maxZoom;
    
    // variable to divide the amount of zoom by
    public float zoomLimiter;

    private Vector3 velocity;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // want to move the camera after everything else
    void LateUpdate()
    {
        // error handling for no players on screen
        if (players.Count == 0)
            return;

        Move();
        Zoom();
    }

    void Move()
    {
        centerPoint = GetCenterPoint();

        newPos = centerPoint + offset;

        // makes the camera movement look pretty
        transform.position = Vector3.SmoothDamp(transform.position, newPos, ref velocity, smoothTime);
    }

    void Zoom()
    {
        // makes the camera zoom look pretty
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime);
    }

    // gets the distance between the two farthest players (lol theres only two tho)
    float GetGreatestDistance()
    {
        // gets the box encapsulating all players
        var bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 0; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }

        // the width of this box is the greatest distance
        return bounds.size.x;
    }

    // 
    Vector3 GetCenterPoint()
    {
        // dont need to do this if theres only one player
        if (players.Count == 1)
        {
            return players[0].position;
        }

        // gets the box encapsulating all players
        var bounds = new Bounds(players[0].position, Vector3.zero);
        for (int i = 0; i < players.Count; i++)
        {
            bounds.Encapsulate(players[i].position);
        }

        // returns the center of this bounding box
        return bounds.center;
    }

}
