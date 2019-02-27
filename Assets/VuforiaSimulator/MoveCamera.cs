/*==============================================================================
* Class: MoveCamera
* Script to attach to the Camera to move it via key press and rotate by
* moving mouse while holding down RMB.
* Author: Foo Jing Ting
* Date: 27 February 2019
==============================================================================*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {

    [SerializeField]
    private KeyCode keyForward = KeyCode.W;
    [SerializeField]
    private KeyCode keyBackward = KeyCode.S;
    [SerializeField]
    private KeyCode keyLeft = KeyCode.A;
    [SerializeField]
    private KeyCode keyRight = KeyCode.D;

    [SerializeField]
    private float moveSpeed = 1f;

    private float yawSpeed = 25f;
    private float pitchSpeed = 25f;
    
    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {

#if UNITY_EDITOR
        // rotate camera while holding RMB
        if (Input.GetMouseButton(1))    // RMB held down
        {
            if (Input.GetMouseButtonDown(1))    // frame in which it was set
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
            
            Vector3 eulerAngles = transform.eulerAngles;
            eulerAngles.x -= pitchSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
            eulerAngles.y += yawSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
            transform.eulerAngles = eulerAngles;
        }
        else if (Input.GetMouseButtonUp(1)) // frame in which RMB no longer pressed
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // move via movement keys
        if (Input.GetKey(keyForward)) {
            transform.position += moveSpeed * transform.forward;
        }
        if (Input.GetKey(keyBackward)) {
            transform.position -= moveSpeed * transform.forward;
        }
        if (Input.GetKey(keyLeft)) {
            transform.position -= moveSpeed * transform.right;
        }
        if (Input.GetKey(keyRight)) {
            transform.position += moveSpeed * transform.right;
        }
    }
#endif
}
