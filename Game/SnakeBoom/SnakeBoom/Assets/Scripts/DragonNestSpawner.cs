using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DragonNestSpawner : NetworkBehaviour {
    public GameObject dragonNest;
    public Transform dragonNestTf;
    public override void OnStartServer() {
        if (!isServer)
            return;
        GameObject ob = Instantiate(dragonNest, dragonNestTf.position, dragonNestTf.rotation);
        NetworkServer.Spawn(ob);
    }
}
