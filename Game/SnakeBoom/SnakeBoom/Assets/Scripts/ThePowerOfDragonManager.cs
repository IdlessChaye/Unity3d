using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class ThePowerOfDragonManager : NetworkBehaviour {
    public GameObject GOFireFX;

    [SyncVar] public NetworkInstanceId whoGet;
    private void Start() {
        whoGet = new NetworkInstanceId((uint)0);
        CmdSpawnFireFX();
    }
    [Command]
    public void CmdSetWhoGet(NetworkInstanceId netId) {
        print("NewOwner: " + netId);
        whoGet = netId;
    }
    [Command]
    private void CmdSpawnFireFX() {
        GameObject fireFX = Resources.Load("FireFX") as GameObject;
        GOFireFX = Instantiate(fireFX, transform.position, transform.rotation);
        NetworkServer.Spawn(GOFireFX);
    }
    public void SetFireFX(Vector3 position, Quaternion rotation) {
        if (!isServer) {
            print("InServer");
            return;
        }
        GOFireFX.transform.position = position;
        GOFireFX.transform.rotation = rotation;
    }
}