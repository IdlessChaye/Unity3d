using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadListManager : MonoBehaviour {
    private static DeadListManager deadListManager;
    public static DeadListManager DLM {
        get {
            return deadListManager;
        }
    }

    public GameObject[] deadLists;

    private float length;
    private float level;

    private void Awake() {
        deadListManager = this;
    }
    private void Start() {
        length = (float)ChessboardManager.Length;
        level = (float)ChessboardManager.Level;
        deadLists = new GameObject[2];
        deadLists[0] = new GameObject("WhiteDeadList");
        DeadList dl = deadLists[0].AddComponent<DeadList>();
        dl.isWhite = true;
        dl.Position = new Vector3((level + 1) / 2 * length, 0, -(level + 1) / 2 * length);
        dl.Direction = Vector3.forward;
        dl.step = length / 2.2f;
        deadLists[1] = new GameObject("BlackDeadList");
        dl = deadLists[1].AddComponent<DeadList>();
        dl.isWhite = false;
        dl.Position = new Vector3(-(level + 1) / 2 * length, 0, (level + 1) / 2 * length);
        dl.Direction = -Vector3.forward;
        dl.step = length / 2.2f;
    }
    private void Add(GameObject dead) {
        deadLists[dead.GetComponent<ChessPieceInfo>().isWhite ? 0 : 1].SendMessage("Add", dead);
    }
    private void Remove(GameObject dead) {
        deadLists[dead.GetComponent<ChessPieceInfo>().isWhite ? 0 : 1].SendMessage("Remove", dead);
    }
}
