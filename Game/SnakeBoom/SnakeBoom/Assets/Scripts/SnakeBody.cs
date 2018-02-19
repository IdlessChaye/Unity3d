using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SnakeBody : NetworkBehaviour {

    public GameObject forwardSnakeBody;
    public float speed;
    public bool onGround;
    public GameObject mySnake;
    public bool isShock;

    private Transform targetTf;
    private Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        SetTargetTf(forwardSnakeBody);
        speed = 7f;
    }
    public void AddSpeed(float speedPlus) {
        if (!hasAuthority)
            return;
        speed += speedPlus;
        if (speed > 10f)
            speed = 10f;
        print(speed);
    }
    private void FixedUpdate() {
        if (!hasAuthority)
            return;
        if (isShock) {
            return;
        }
        if (onGround == true) {
            rb.velocity = new Vector3(rb.velocity.x, -.001f, rb.velocity.z);
        }
        if (targetTf != null) {
            if (Vector3.Distance(targetTf.position, transform.position) > 2f) {
                transform.LookAt(targetTf);
                rb.angularVelocity = Vector3.zero;
                rb.velocity -= Vector3.Project(rb.velocity, transform.right);
                if (rb.velocity.magnitude < 15f) {
                    rb.AddForce(transform.forward * 500f);
                } else {
                    rb.velocity = rb.velocity.normalized * 10f;
                }
            } else {
                if (rb.velocity.magnitude > speed) {
                    rb.velocity = rb.velocity.normalized * speed;
                }
            }
        }
    }
    private void Update() {
        if (!hasAuthority)
            return;
        if (isShock)
            return;
        DetectOnGround();
    }
    private void SetTargetTf(GameObject ob) {
        if (ob != null) {
            forwardSnakeBody = ob;
            targetTf = ob.transform;//得到上一个目标
        }
    }
    private void CancelSetTargetTf() {
        forwardSnakeBody = null;
        targetTf = null;
    }
    private void DetectOnGround() {
        onGround = false;
        Collider[] colliders;
        float radius = 0.95f;
        if (GetComponent<SnakeHead>() != null) {//这截身体是脑袋
            radius = 1.45f;
        }
        colliders = Physics.OverlapBox(transform.position, transform.localScale / radius);
        if (colliders.Length != 0) {
            foreach (Collider collider in colliders) {
                if (collider.tag == "Ground") {
                    onGround = true;
                    break;
                }
            }
        }
    }
    [ClientRpc]
    private void RpcTakeDamage(float damageValue) {
        if (!hasAuthority)
            return;
        print("TakeDmage InSnake");
        mySnake.GetComponent<Snake>().CmdTakeDamage(damageValue);
    }
}
