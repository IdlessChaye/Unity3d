using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FuSpawner : NetworkBehaviour {
    public GameObject[] fus;
    public Transform[] spawnTf;
    public float timeInterval;
    public NetworkInstanceId[] hasSpawnedFus;

    private float lastSpawnTime;
    private NetworkInstanceId netIdZero;
    private void Start() {
        netIdZero = new NetworkInstanceId((uint)0);
        lastSpawnTime = -timeInterval;
        hasSpawnedFus = new NetworkInstanceId[spawnTf.Length];
        for (int i = 0; i < hasSpawnedFus.Length; ++i) {
            hasSpawnedFus[i] = netIdZero;
        }
    }
    private void Update() {
        if (!isServer)
            return;
        if (Time.time - lastSpawnTime > timeInterval) {
            //if ((int)Time.time % (int)timeInterval == 0) {//每隔timeInterval,执行一次
            List<int> spawnPositionIndexList = new List<int>();
            for (int i = 0; i < spawnTf.Length; ++i) {
                spawnPositionIndexList.Add(i);
            }
            int index = -1;
            do {
                index = spawnPositionIndexList[Random.Range(0, spawnPositionIndexList.Count)];
                spawnPositionIndexList.Remove(index);
            } while (hasSpawnedFus[index] != netIdZero && spawnPositionIndexList.Count != 0);//如果这个位置有符且有地方，就再次找下一个位置
            if (hasSpawnedFus[index] == netIdZero) {//如果这个位置空着
                GameObject fu = Instantiate(fus[Random.Range(0, fus.Length)], spawnTf[index].position, spawnTf[index].rotation);
                NetworkServer.Spawn(fu);
                hasSpawnedFus[index] = fu.GetComponent<NetworkIdentity>().netId;
            }
            lastSpawnTime = Time.time;
        }
    }
    [Command]
    public void CmdClearIndexOfHasSpawnedFu(NetworkInstanceId netId) {
        for (int i = 0; i < hasSpawnedFus.Length; ++i) {
            if (hasSpawnedFus[i] == netId) {
                hasSpawnedFus[i] = netIdZero;
                Destroy(NetworkServer.FindLocalObject(netId));
                break;
            }
        }
    }

    public void EatSpawnFu(NetworkInstanceId netId) {
        if (!isServer)
            return;
        int index = -1;
        for (int i = 0; i < hasSpawnedFus.Length; ++i) {
            if (hasSpawnedFus[i] == netId) {
                index = i;
                break;
            }
        }
        GameObject boom = Resources.Load("Boom") as GameObject;
        foreach (Transform element in spawnTf[index]) {
            GameObject boomFood = Instantiate(boom, element.position, element.rotation) as GameObject;
            NetworkServer.Spawn(boomFood);
        }
    }
}
