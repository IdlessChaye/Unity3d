using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ThrowBoom : NetworkBehaviour {
    private Vector3 forward;
    private Vector3 right;
    private Rigidbody rb;
    private bool readyToBoom;
    private bool isBoom;
    [SyncVar] private float damageValue;
    [Command]
    private void CmdSetForwardDirection(Vector3 forward) {
        this.forward = forward;
    }
    [Command]
    private void CmdSetRightDirection(Vector3 right) {
        this.right = right;
    }
    [Command]
    private void CmdThrow(float force) {//400f-1200f 800f 此时这个boom已经荣誉的省纪委服务器版本的了 请你同步。。
        rb = GetComponent<Rigidbody>();
        Quaternion upAngel = Quaternion.AngleAxis(-60f, right);
        rb.AddForce(upAngel * forward * force);
        rb.angularVelocity = Vector3.zero;
        readyToBoom = false;
        Invoke("ReadyToBoom", 0.3f);
        isBoom = tag == "BoomCaptured" ? true : false;
    }
    void ReadyToBoom() {
        readyToBoom = true;
    }
    [Command]
    public void CmdSetDamageValue(float damageValue) {
        this.damageValue = damageValue;
        //print("In CmdSetDamageValue" + GetComponent<NetworkIdentity>().netId+damageValue);
    }
    [Command]
    public void CmdBoom(bool isUnderControl) {//是否是在控制下,引爆，而不是撞到什么自爆
        gameObject.tag = "Finish";
        GameObject boom = gameObject;
        float length = boom.transform.localScale.x;
        float quaterLength = length / 4;
        GameObject boomSegment = Resources.Load("BoomSegment") as GameObject;
        Vector3 position;
        position = boom.transform.position;
        float xPosition = (position - length * 3 / 8 * transform.right).x;
        float zPosition = (position - length * 3 / 8 * boom.transform.forward).z;
        float yPosition = (position - length * 3 / 8 * boom.transform.up).y;
        for (int i = 0; i < 4; ++i) {
            for (int j = 0; j < 4; ++j) {
                position = new Vector3(xPosition, yPosition, zPosition) + i * quaterLength * boom.transform.up;
                position += j * quaterLength * boom.transform.right;
                for (int k = 0; k < 4; ++k) {
                    position += k * quaterLength * boom.transform.forward;
                    GameObject go = Instantiate(boomSegment, position, boom.transform.rotation);//服务器创建，服务器同步
                    NetworkServer.Spawn(go);
                }
            }
        }
        Vector3 boomPosition = boom.transform.position;
        Vector3 boomForward = boom.transform.forward;
        Vector3 boomUp = boom.transform.up;
        NetObjectId.Objects.Remove(boom);
        Destroy(boom);

        Collider[] colliders;
        colliders = Physics.OverlapBox(boomPosition, Vector3.one * 5f);
        foreach (Collider collider in colliders) {
            switch (collider.tag) {
                case "BoomUnCaptured":
                case "BoomCaptured":
                case "Body":
                case "BoomSegment":
                    Vector3 explosionPosition = boomPosition;
                    if (isUnderControl) {
                        explosionPosition += -boomForward * length + boomUp * length / 2;
                    }
                    collider.GetComponent<ExplosionForce>().CmdAddExplosionForce(explosionPosition, damageValue);//这个要改，服务器版本，但是对于snakePart客户端也需要这样，所以就MonoBehavior吧
                    print("In CmdAddExplosionForce" + GetComponent<NetworkIdentity>().netId + damageValue);
                    break;
            }
        }
    }
    [Command]
    public void CmdBounceQuad() {
        Vector3 direction = transform.forward + transform.up;
        Vector3 position = transform.position;
        GameObject bounceQuad = Resources.Load("BounceQuad") as GameObject;
        GameObject quadForward = Instantiate(bounceQuad, position, Quaternion.LookRotation(direction));
        NetworkServer.Spawn(quadForward);
        GameObject quadBackward = Instantiate(bounceQuad, position, Quaternion.LookRotation(-direction));
        NetworkServer.Spawn(quadBackward);

        NetObjectId.Objects.Remove(gameObject);
        Destroy(gameObject);
    }


    private void OnCollisionStay(Collision collision) {
        if (!isServer)
            return;
        if (!readyToBoom)
            return;
        if (isBoom) {
            CmdBoom(false);
        } else {
            CmdBounceQuad();
        }
    }
}
