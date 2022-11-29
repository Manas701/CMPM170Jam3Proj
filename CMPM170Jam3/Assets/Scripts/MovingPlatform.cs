using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public enum dirEnum{ Up, Down, Left, Right };
    [SerializeField] public GameObject platformObject;
    [SerializeField] public dirEnum moveDirection;
    [SerializeField] public float moveSpeed;
    [SerializeField] public float moveDistance;

    private Vector2 origPos;
    private float endPos;
    private bool moveTowardsEnd;

    // Start is called before the first frame update
    void Start()
    {
        origPos = platformObject.transform.position;

        if (moveDirection == dirEnum.Up){
            endPos = origPos.y + moveDistance;
        }
        else if (moveDirection == dirEnum.Down){
            endPos = origPos.y - moveDistance;
        }
        else if (moveDirection == dirEnum.Left){
            endPos = origPos.x - moveDistance;
        }
        else if (moveDirection == dirEnum.Right){
            endPos = origPos.x + moveDistance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (moveDirection == dirEnum.Up){
            if (moveTowardsEnd && (platformObject.transform.position.y <= endPos)) {
                platformObject.transform.position = new Vector2(origPos.x, (platformObject.transform.position.y + (moveSpeed / 50)));
            }
            else if (!moveTowardsEnd && (platformObject.transform.position.y >= origPos.y)){
                platformObject.transform.position = new Vector2(origPos.x, (platformObject.transform.position.y - (moveSpeed / 50)));
            }
        }
        else if (moveDirection == dirEnum.Down){
            if (moveTowardsEnd && (platformObject.transform.position.y >= endPos)) {
                platformObject.transform.position = new Vector2(origPos.x, (platformObject.transform.position.y - (moveSpeed / 50)));
            }
            else if (!moveTowardsEnd && (platformObject.transform.position.y <= origPos.y)){
                platformObject.transform.position = new Vector2(origPos.x, (platformObject.transform.position.y + (moveSpeed / 50)));
            }
        }
        else if (moveDirection == dirEnum.Left){
            if (moveTowardsEnd && (platformObject.transform.position.x >= endPos)) {
                platformObject.transform.position = new Vector2((platformObject.transform.position.x - (moveSpeed / 50)), origPos.y);
            }
            else if (!moveTowardsEnd && (platformObject.transform.position.x <= origPos.x)){
                platformObject.transform.position = new Vector2((platformObject.transform.position.x + (moveSpeed / 50)), origPos.y);
            }
        }
        else if (moveDirection == dirEnum.Right){
            if (moveTowardsEnd && (platformObject.transform.position.x <= endPos)) {
                platformObject.transform.position = new Vector2((platformObject.transform.position.x + (moveSpeed / 50)), origPos.y);
            }
            else if (!moveTowardsEnd && (platformObject.transform.position.x >= origPos.x)){
                platformObject.transform.position = new Vector2((platformObject.transform.position.x - (moveSpeed / 50)), origPos.y);
            }
        }
    }

    void OnTriggerStay2D (Collider2D other){
        if (other.CompareTag("Player")){
            moveTowardsEnd = true;
            if (!GameObject.Find("Main Camera").GetComponent<FollowPlayers>().targets.Contains(platformObject.transform))
            {
                GameObject.Find("Main Camera").GetComponent<FollowPlayers>().targets.Add(platformObject.transform);
            }
        }
     }
 
    void OnTriggerExit2D (Collider2D other){
        if (other.CompareTag("Player")){
            moveTowardsEnd = false;
            if (GameObject.Find("Main Camera").GetComponent<FollowPlayers>().targets.Contains(platformObject.transform))
            {
                GameObject.Find("Main Camera").GetComponent<FollowPlayers>().targets.Remove(platformObject.transform);
            }
        }
     }
}
