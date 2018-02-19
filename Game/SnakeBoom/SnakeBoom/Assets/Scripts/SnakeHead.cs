using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class SnakeHead : NetworkBehaviour {
    public GameObject mySnake;
    public GameObject myClientCamera;
    public bool isShock;

    private Rigidbody rb;
    private bool onGround;
    private float speed;
    private float jumpForce;
    private bool isDragon;
    private bool powerOfDragon;
    private void Start() {
        rb = GetComponent<Rigidbody>();
        onGround = false;
        speed = 7f;
        jumpForce = 500f;
    }
    public void AddSpeed(float speedPlus) {
        if (!hasAuthority)
            return;
        speed += speedPlus;
        if (speed > 15f)
            speed = 15f;
        print(speed);
    }
    private void FixedUpdate() {
        if (!hasAuthority) {
            //print("HeadInID: " + gameObject.GetComponent<NetworkIdentity>().netId + " mySnake: " + mySnake);//能好好用
            return;
        }
        if (isShock) {
            return;
        }
        Vector3 cameraRight = myClientCamera.transform.right;
        onGround = GetComponent<SnakeBody>().onGround;
        rb.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(-90f, Vector3.up) * cameraRight);
        rb.angularVelocity = Vector3.zero;
        rb.velocity -= Vector3.Project(rb.velocity, transform.right);
        //  print("RBSPEED: "+rb.velocity); //不能走是因为控制权还在server
        if (rb.velocity.magnitude < speed) {
            rb.AddForce(transform.forward * 300f);
        }
        if (powerOfDragon) {
            //if (Input.GetKeyDown(KeyCode.D)) {
            //    SetIsDragon(!isDragon);
            //}
            mySnake.GetComponent<Snake>().CmdSetFireFX(transform.position, transform.rotation);
        }
        if (Input.GetKeyDown(KeyCode.Space) && (onGround == true || isDragon)) {
            Jump();
        }
        if (onGround == true) {
            rb.velocity = new Vector3(rb.velocity.x, -.001f, rb.velocity.z);
        } else {
            rb.velocity -= Vector3.up * 0.01f;
        }
    }
    private void Jump() {
        if (isDragon == false && transform.localPosition.y > 7)
            return;
        rb.AddForce((Vector3.up + transform.forward / 5) * jumpForce);
        onGround = false;
    }
    private void SetIsDragon(bool isDragon) {
        this.isDragon = isDragon;
    }
    public void SetThePowerOfDragon(bool isOwn) {
        isDragon = powerOfDragon = isOwn;
    }
    private void OnCollisionStay(Collision collision) {//只在服务器获得新脑袋？好像在客户端也行，不行，只能在客户端，因为mySnake
        if (!hasAuthority)
            return;
        string tag = collision.gameObject.tag;
        if (tag == "BoomUnCaptured") {
            Snake mySnakeScript = mySnake.GetComponent<Snake>();
            if (mySnakeScript.isHungry == true) {
                mySnakeScript.isHungry = false;
                Transform tf = collision.gameObject.transform;
                mySnakeScript.CmdCreatBoom(collision.gameObject.GetComponent<NetworkIdentity>().netId, tf.position, tf.rotation);//不同地点的游戏物体传递要通过id，不能直接传要不null
            }
        }
    }
    private void OnTriggerEnter(Collider other) {
        if (!hasAuthority)
            return;
        string tag = other.tag;
        switch (tag) {
            case "Fu":
                FuMode thisFuMode = other.GetComponent<FuManager>().thisFuMode;
                print(thisFuMode);
                NetworkInstanceId netId = other.GetComponent<NetworkIdentity>().netId;
                switch (thisFuMode) {
                    case FuMode.Attack:
                        mySnake.GetComponent<Snake>().CmdEatAttackFu(5f);
                        break;
                    case FuMode.Recovery:
                        mySnake.GetComponent<Snake>().CmdEatRecoveryFu();
                        break;
                    case FuMode.Spawn:
                        CmdEatSpawnFu(netId);
                        break;
                    case FuMode.Speed:
                        mySnake.GetComponent<Snake>().EatSpeedFu(2f);
                        break;
                }
                CmdFuClear(netId);
                break;
            case "DragonNest":
                ThePowerOfDragonManager tpodm = other.GetComponent<ThePowerOfDragonManager>();
                NetworkInstanceId whoGetNetId = mySnake.GetComponent<NetworkIdentity>().netId;
                if (whoGetNetId != tpodm.whoGet) {
                    mySnake.GetComponent<Snake>().CmdSetWhoGet(whoGetNetId);
                    mySnake.GetComponent<Snake>().CmdGiveThePowerOfDragon(whoGetNetId);
                }
                break;
        }
    }

    [Command]
    void CmdEatSpawnFu(NetworkInstanceId netId) {
        GameObject.FindWithTag("FuSpawner").GetComponent<FuSpawner>().EatSpawnFu(netId);
    }
    [Command]
    void CmdFuClear(NetworkInstanceId netId) {
        FuSpawner fuSpawner = GameObject.FindWithTag("FuSpawner").GetComponent<FuSpawner>();
        fuSpawner.CmdClearIndexOfHasSpawnedFu(netId);
    }
}
