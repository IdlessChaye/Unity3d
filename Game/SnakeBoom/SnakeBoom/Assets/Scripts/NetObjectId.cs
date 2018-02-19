using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class NetObjectId : NetworkBehaviour {
    [SyncVar(hook = "SetID")]
    //[HideInInspector]
    public int NetID;

    private static int netObjectsCount;

    public static List<GameObject> Objects;

    //  [ServerCallback]
    private void Start() {
        if (Objects == null)
            Objects = new List<GameObject>();

        netObjectsCount = Objects.Count;
        NetID = netObjectsCount + 1;


        Objects.Add(gameObject);

    }

    void SetID(int value) {
        NetID = value;
    }
}