using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetsDropTheBeat : MonoBehaviour {
    public GameObject[] jumpPieces;
    public float jumpInterval;
    private float lastJumpTime;
    private AudioSource audio;
    private float[] spectrum = new float[128];
    int count;
    private void Awake() {
        audio = GetComponent<AudioSource>();
        lastJumpTime = -jumpInterval;
    }
    private void Update() {
        if (UIManager.UIM.gameStart == false) {
            audio.GetOutputData(spectrum, 0);
            float max, all = 0;
            max = Mathf.Log(spectrum[1 - 1]);
            for (int i = 1; i < spectrum.Length - 1; i++) {
                if (Mathf.Log(spectrum[i - 1]) > max)
                    max = Mathf.Log(spectrum[i - 1]);
                Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]), 2f), new Vector3(i, Mathf.Log(spectrum[i]), 2f), Color.cyan);
                all += spectrum[i];
            }
            if (all > 7f) {
                Boom();
            }
        }
    }
    private int i = 0;
    private string[] waveType = { "Boom", "Pawn", "Bishop", "Knight", "Queen", "Rook" };
    void Boom() {
        if (Time.time - lastJumpTime > jumpInterval) {
            i += Random.Range(0, 3);
            i %= jumpPieces.Length;
            jumpPieces[i].SendMessage("Jump");
            lastJumpTime = Time.time;
        }
        count = ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary.Count;
        GameObject[] meshArray = new GameObject[count];
        ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary.Values.CopyTo(meshArray, 0);
        meshArray[Random.Range(0, count)].SendMessage("StartWave", waveType[Random.Range(0, waveType.Length)]);
    }
}
