using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class BoomSpawner : NetworkBehaviour {
    public GameObject[] booms;
    public float boomRefreshInterval;

    private float lastTime;
    private void Awake() {
        lastTime = -boomRefreshInterval;
    }
    private void Update() {
        if (!isServer)
            return;
        if (Time.time - lastTime > boomRefreshInterval) {
            float xPosition = Random.Range(-70, 70);
            float zPosition = Random.Range(-70, 70);
            Ray ray = new Ray(new Vector3(xPosition, -50f, zPosition), Vector3.up);
            Vector3 groundPoint = Vector3.zero;
            RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
            if (hits.Length != 0) {
                for (int i = 0; i < hits.Length; ++i) {
                    if (hits[i].collider.tag == "Ground") {
                        groundPoint = hits[i].point;
                        break;
                    }
                }
            }
            GameObject boomFood = Instantiate(booms[Random.Range(0, booms.Length)], new Vector3(xPosition, Random.Range(0, 3f) + groundPoint.y, zPosition), Quaternion.identity) as GameObject;
            boomFood.name = "BoomFood";
            NetworkServer.Spawn(boomFood);
            lastTime = Time.time;
        }
    }
}
