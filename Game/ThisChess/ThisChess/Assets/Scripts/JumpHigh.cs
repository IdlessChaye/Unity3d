using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpHigh : MonoBehaviour {
    public GameObject nextJump;
    public float force=3000f;
    public float velocity = 10f;
    private Rigidbody rb;
    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }
    void Jump() {
        rb.velocity += Vector3.up * velocity;
        if (nextJump != null)
            Invoke("NextJump", 0.01f);
    }
    void NextJump() {
        nextJump.SendMessage("Jump");
    }
}
