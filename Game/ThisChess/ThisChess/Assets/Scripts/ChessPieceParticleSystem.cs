using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPieceParticleSystem : MonoBehaviour {
    public GameObject segmentParticle;
    public GameObject glitter2Particle;

    private bool isParticle;

    public static bool ChessPieceFX { get; set; }
    private void SetBools() {
        ChessPieceFX = true;
    }
    void Awake() {
        isParticle = false;
        SetBools();
    }
    void Update() {
        if (isParticle) {
            gP.transform.LookAt(Camera.main.transform);
        }
    }

    private GameObject sP, gP;
    private void OnParticleSystem(bool isParticleSystem) {
        if (ChessPieceFX) {
            isParticle = isParticleSystem;
            if (isParticle) {
                float upAmount = 2;
                if (gameObject.tag == "Pawn" || gameObject.tag == "Rook")
                    upAmount = 1.5f;
                Vector3 position = ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary[gameObject.GetComponent<ChessPieceInfo>().Position].transform.position;
                sP = Instantiate(segmentParticle,position + Vector3.up * 4, Quaternion.AngleAxis(90f, Vector3.right));
                gP = Instantiate(glitter2Particle, position + Vector3.up * upAmount, Quaternion.AngleAxis(90f, Vector3.right));
            } else {
                Destroy(sP);
                Destroy(gP);
            }
        }
    }
}
