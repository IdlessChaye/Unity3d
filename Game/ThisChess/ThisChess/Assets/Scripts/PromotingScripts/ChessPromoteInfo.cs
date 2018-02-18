using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPromoteInfo : ChessMoveInfo {
    public string fromTag;
    public string toTag;
    public ChessPromoteInfo(bool isWhite, GameObject chessPiece, Vector3 from, Vector3 to,string fromTag,string toTag) : base(isWhite, chessPiece, from, to) {
        this.fromTag = fromTag;
        this.toTag = toTag;
    }
}
