using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
    public string nextScene;
    public bool fakeDoor = false;
    private bool p1Collided = false;
    private bool p2Collided = false;

    // Update is called once per frame
    void Update()
    {
        if (p1Collided && p2Collided){
            if (!fakeDoor){
                SceneManager.LoadScene(nextScene);
            }
            else{
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        if (Input.GetKeyDown(KeyCode.P)){
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //Debug.Log("P pressed");
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.layer == 7) {
            p1Collided = true;
        }
        if (other.gameObject.layer == 8) {
            p2Collided = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.layer == 7) {
            p1Collided = false;
        }
        if (other.gameObject.layer == 8) {
            p2Collided = false;
        }
    }
}
