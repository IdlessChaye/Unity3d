using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessMoveInfo {
    public bool isWhite;
    public GameObject chessPiece;
    public Vector3 from;
    public Vector3 to;

    public ChessMoveInfo(bool isWhite,GameObject chessPiece, Vector3 from ,Vector3 to) {
        this.isWhite = isWhite;
        this.chessPiece = chessPiece;
        this.from = from;
        this.to = to;
    }
};
