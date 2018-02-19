using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ExplosionForce : NetworkBehaviour {
    public float force = 50f;
    SnakeBody sb;
    SnakeHead sh;

    [Command]
    public void CmdAddExplosionForce(Vector3 exploPosition, float damageValue) {//服务器和客户端都同步就不用管了吧,不行 不行不行 服务器同步
        switch (gameObject.tag) {//只有蛇身可以接触控制
            case "Body":
            case "BoomCaptured":
                RpcShutDown(exploPosition);
                RpcInvokeEnabledTrue();
                break;
            case "BoomSegment":
                print("InExplosionForce" + GetComponent<NetworkIdentity>().netId + " "+damageValue);
                GetComponent<ExplosionDamage>().SetDamageValue(damageValue);
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.AddExplosionForce(force, exploPosition, 13f);
                }
                break;
        }
    }

    [ClientRpc]
    void RpcShutDown(Vector3 exploPosition) {//在客户端控制停止蛇身
        if (!hasAuthority)
            return;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) {
            rb.AddExplosionForce(force, exploPosition, 13f);
        }
        sb = GetComponent<SnakeBody>();
        sh = GetComponent<SnakeHead>();
        if (sb != null)
            sb.isShock = true;
        if (sh != null) {
            sh.isShock = true;
        }
    }
    [ClientRpc]
    void RpcInvokeEnabledTrue() {
        if (!hasAuthority)
            return;
        Invoke("EnabledTrue", 1f);
    }
    private void EnabledTrue() {
        if (!hasAuthority)
            return;
        sb = GetComponent<SnakeBody>();
        sh = GetComponent<SnakeHead>();
        if (sb != null) {
            sb.isShock = false;
        }
        if (sh != null) {
            sh.isShock = false;
        }
    }
}
