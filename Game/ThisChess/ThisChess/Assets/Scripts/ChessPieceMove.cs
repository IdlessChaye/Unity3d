using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPieceController : MonoBehaviour {
    public float quatSmoothing = 0.003f;
    public float moveASpeed = 1000f;
    public float maxVelocity = 0f;
    public bool IsLocked {
        get {
            return isLocked;
        }
        set {
            isLocked = value;
        }
    }
    private bool isLocked; // 就是被摄像机看着呢
    private Rigidbody targetRb;
    private Vector3 moveDistance;
    private float right, forward;
    void Awake() {
        isLocked = false; 
        targetRb = GetComponent<Rigidbody>();
        moveDistance = Vector3.zero;
    }
    private void FixedUpdate() {
        if (isLocked) {
            if (moveDistance != Vector3.zero) {
                targetRb.AddForce(moveDistance.normalized * Time.deltaTime * moveASpeed);
                targetRb.MoveRotation(Quaternion.Lerp(targetRb.rotation, Quaternion.LookRotation(moveDistance), quatSmoothing * Time.time));
            }
            if (targetRb.velocity.magnitude > maxVelocity)
                targetRb.velocity = targetRb.velocity.normalized * maxVelocity;
        }
    }
    private void Update() {
        if (isLocked) {
            forward = Input.GetAxis("Vertical");
            right = Input.GetAxis("Horizontal");
            moveDistance = Vector3.zero;
            if (forward != 0)
                moveDistance += Camera.main.transform.forward * forward;
            if (right != 0)
                moveDistance += Camera.main.transform.right * right;
            if (Input.GetKey(KeyCode.E))
                moveDistance += Vector3.up;
            if (Input.GetKey(KeyCode.Q))
                moveDistance -= Vector3.up;
        }
    }
}
