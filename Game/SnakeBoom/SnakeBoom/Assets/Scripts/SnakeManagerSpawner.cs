using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class SnakeManagerSpawner : NetworkBehaviour {
    public override void OnStartServer() {
        GameObject snakeManagerInServer = Resources.Load("SnakeManagerInServer") as GameObject;
        GameObject snakeSpawn = Instantiate(snakeManagerInServer, Vector3.zero, Quaternion.identity);
        NetworkServer.Spawn(snakeSpawn);
    }
}
