using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ExplosionDamage : NetworkBehaviour {//只有BoomSegment吧 在服务器处理
    [SerializeField] private bool destroyAfterCreated;
    [SerializeField] private float destroyDelayTime;
    [SyncVar] public float damageValue;
    public void SetDamageValue(float damageValue) {
        if (!isServer)
            return;
        this.damageValue = damageValue;
        print("In SetDamageValue " + GetComponent<NetworkIdentity>().netId +" " + this.damageValue);
    }
    private void Start() {
        if (!isServer)
            return;
        if (destroyAfterCreated) {
            Invoke("DestroyDelay", destroyDelayTime);
        }
    }
    private void OnCollisionEnter(Collision collision) {
        if (!isServer)
            return;
        string tag = collision.transform.tag;
        if (tag == "BoomSegment" || tag == "BounceQuad") {
            return;
        }
        if ((tag == "Body" || tag == "BoomCaptured") && collision.gameObject.GetComponent<SnakeBody>() != null) {//是条蛇
            print("RpcTakeDamage" + collision.collider.GetComponent<NetworkIdentity>().netId + "Damage" + damageValue);
            //collision.collider.SendMessage("RpcTakeDamage", damageValue);
            CmdTakeDamageInManager(collision.gameObject.GetComponent<NetworkIdentity>().netId,damageValue); 
        }
        NetObjectId.Objects.Remove(gameObject);
        Destroy(gameObject);

    }
    [Command]
    void CmdTakeDamageInManager (NetworkInstanceId netId,float damageValue) {
        GameObject snakeManager = GameObject.FindWithTag("SnakeManagerInServer");
        SnakeManagerInServer snakeManagerInServer = snakeManager.GetComponent<SnakeManagerInServer>();
        snakeManagerInServer.CmdTakeDamageInServer(netId,damageValue);
    }
    private void DestroyDelay() {
        if (!isServer)
            return;
        NetObjectId.Objects.Remove(gameObject);
        Destroy(gameObject);
    }
}
