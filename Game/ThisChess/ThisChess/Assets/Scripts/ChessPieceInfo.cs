using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPieceInfo : MonoBehaviour {

    [SerializeField] private Vector3 position;
    public Vector3 Position {
        get {
            return position;
        }
        set {
            position = value;
        }
    }
    public bool isWhite;
    public bool dontMove = true;

    private GameObject queenModel;
    private Material material;
    private bool UNITYBUGBUGBUGAI = true; // 为什么要有这个bool呢？因为Start的执行顺序飘忽不定啊！！
    private bool lerpingPosition;
    private Vector3 targetPosition;
    private float smoothing;
    private bool isDraw;
    private float drawInterval = 2f;
    private float lastDrawTime;
    private int layerPlayer;
    private Vector3 startPosition;
    private void Awake() {
        if (isWhite) {
            material = Resources.Load("Materials/PlayerWhite") as Material;
            GetComponent<MeshRenderer>().sharedMaterial = material;
        } else {
            material = Resources.Load("Materials/PlayerBlack") as Material;
            GetComponent<MeshRenderer>().sharedMaterial = material;
        }
        isDraw = false;
        layerPlayer = LayerMask.GetMask("Player");
        smoothing = 3f;
        startPosition = position;
        if (dontMove) {
            GetComponent<Rigidbody>().Sleep();
            GetComponent<ChessPieceController>().enabled = false;
        }
    }
    private void Start() {
        GetComponent<MeshRenderer>().enabled = false;//MainManu开始，要求隐藏棋子，游戏开始再激活
    }
    Ray ray;
    RaycastHit hit;
    private void Update() {
        if (Time.time > 0.02f && UNITYBUGBUGBUGAI) {
            StartLate();
        }
        if (UIManager.UIM.chessPieceDisplay || UIManager.UIM.gameStart) {//游戏开始时一定可以展开?对的
            if (lerpingPosition) {
                transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing * Time.deltaTime);
                if (transform.position == targetPosition)
                    lerpingPosition = false;
            }
        }
        if (UIManager.UIM.gameStart) {//在游戏中才能draw
            if (isDraw) {
                if (!Input.GetButton("Fire1"))
                    isDraw = false;
                if (Time.time - lastDrawTime > drawInterval) {
                    ChessManager.CM.SendMessage("OnDraw", isWhite);
                    isDraw = false;
                }
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerPlayer)) {
                    if (hit.collider.tag != "King")
                        isDraw = false;
                } else {
                    isDraw = false;
                }
            }
        }
    }
    private void StartLate() {
        ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[isWhite ? 0 : 1].Add(gameObject);//双方棋子列表
        ChangePosition(this.position);
        UNITYBUGBUGBUGAI = false;
    }
    private void ChangePosition(Vector3 position) {
        if (UNITYBUGBUGBUGAI == false) {
            //print(gameObject.name + " MoveFrom " + Position + " To " + position);
            ChessPieceMoveManager.chessPieceMoveManager.chessPiecesOnChessboardDictionary.Remove(Position);
        }
        Position = position;
        SetTargetPosition(ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary[position].transform.position);//通过坐标确定世界位置
        ChessPieceMoveManager.chessPieceMoveManager.chessPiecesOnChessboardDictionary.Add(position, gameObject);//棋子位置字典
        if (gameObject.tag == "Pawn") { //也要case tag 比如pawn行至底线 变身！（升格）
            int maxIndex = ChessboardManager.Level - (int)Mathf.Abs(position.z);
           // print("MAxIndex: " + maxIndex);
            if (ChessManager.AIPlaying == false) {//玩家的操作
                if (isWhite) {
                    if (position.y == maxIndex) {
                        UIManager.UIM.SendMessage("LabelPromote");
                        PawnPromoteManager.PPM.SendMessage("StartPromotion", gameObject);
                    }
                } else {
                    if (position.y == 1) {
                        UIManager.UIM.SendMessage("LabelPromote");
                        PawnPromoteManager.PPM.SendMessage("StartPromotion", gameObject);
                    }
                }
            } else {//AI的操作
                if (ChessManager.CM.isWhitePlayer) {//player是白色 AI为黑色 终点是零
                    if (position.y == 1) {
                        queenModel = GameObject.Find("QueenModel");
                        //print("Find QueenModel: " + queenModel);
                        UIManager.UIM.SendMessage("LabelPromote");
                        PawnPromoteManager.ChangeAppearance(gameObject, queenModel, "Queen");
                        ChessPromoteInfo chessPromoteInfo = new ChessPromoteInfo(isWhite, gameObject, position, position, "Pawn", "Queen");
                        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("PushChessMoveInfo", chessPromoteInfo);
                        //不用Check 之后在AIShowTIme会Check
                        queenModel = null;
                    }
                } else {
                    if (position.y == maxIndex) {
                        queenModel = GameObject.Find("QueenModel");
                        //print("Find QueenModel: " + queenModel);
                        UIManager.UIM.SendMessage("LabelPromote");
                        PawnPromoteManager.ChangeAppearance(gameObject, queenModel, "Queen");
                        ChessPromoteInfo chessPromoteInfo = new ChessPromoteInfo(isWhite, gameObject, position, position, "Pawn", "Queen");
                        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("PushChessMoveInfo", chessPromoteInfo);
                        queenModel = null;
                    }
                }
            }
        }
    }
    private void DestroyToRest() {
        ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[isWhite ? 0 : 1].Remove(gameObject); //chessPiecesLists除名
                                                                                                          // ChessPieceMoveManager.chessPieceMoveManager.chessPiecesOnChessboardDictionary.Remove(Position);//BUG 这个在OnDestroy中移除有什么区别？
        if (gameObject.tag == "King") { //死亡分支
            ChessManager.CM.SendMessage("WhosWinner", !isWhite);
        }
        DeadListManager.DLM.SendMessage("Add", gameObject);
        SetTargetPosition(DeadListManager.DLM.deadLists[isWhite ? 0 : 1].GetComponent<DeadList>().Position);
    }
    private void SetTargetPosition(Vector3 targetPosition) {
        this.targetPosition = targetPosition;
        lerpingPosition = true;
    }
    private void OnDraw() {
        isDraw = true;
        lastDrawTime = Time.time;
    }
}
