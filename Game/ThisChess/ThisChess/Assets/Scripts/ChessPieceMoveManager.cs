using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ChessPieceMoveManager : MonoBehaviour {
    public static ChessPieceMoveManager chessPieceMoveManager;

    public List<List<GameObject>> chessPiecesLists = new List<List<GameObject>>(){
        new List<GameObject>(),new List<GameObject>() };//0 for White Players
    public Dictionary<Vector3, GameObject> chessboardDictionary = new Dictionary<Vector3, GameObject>();//通过棋盘位置找Background
    public Dictionary<Vector3, GameObject> chessPiecesOnChessboardDictionary = new Dictionary<Vector3, GameObject>();//通过棋盘位置找Player
    public GameObject pawnModel;

    private int level;
    private Ray ray;
    private RaycastHit hit;
    private int layerMaskPlayer;
    private int layerMaskHighlightMesh;
    private int layerMaskSelectedMesh;
    private GameObject chessPiece;
    [HideInInspector] public bool hasHighlightMesh;
    private bool hasSelectedMesh;
    private GameObject selectedMeshGO;
    private Vector3 selectedMeshPosition;
    private GameObject pawnModelNew;

    public static bool WaveFX { get; set; }
    public static bool WaveKillFX { get; set; }
    public static bool RemoveStillCheckMeshFX { get; set; }

    private void SetBools() {
        WaveFX = true;
        WaveKillFX = true;
        RemoveStillCheckMeshFX = true;
    }
    void Awake() {
        chessPieceMoveManager = this;
        level = ChessboardManager.Level;
        layerMaskPlayer = LayerMask.GetMask("Player");
        layerMaskHighlightMesh = LayerMask.GetMask("HighlightBackground");
        layerMaskSelectedMesh = LayerMask.GetMask("SelectedBackground");
        hasHighlightMesh = false;
        hasSelectedMesh = false;
        AddablePawnSearchPoint = 0;
        AddableRookSearchPoint = 0;
        AddableBishopSearchPoint = 0;
        AddableQueenSearchPoint = 0;
        AddableKingSearchPoint = 0;
        AddableKnightSearchPoint = 0;
        SetBools();
    }
    private void Start() {
        pawnModelNew = Instantiate(pawnModel, Vector3.one * 50, Quaternion.identity);
    }

    private bool SwitchPlayer(string tag) {
        switch (tag) {
            case "Pawn":
                if (PlayPawn(chessPiece)) {
                    return true;
                }
                break;
            case "King":
                if (PlayKing(chessPiece)) {
                    return true;
                }
                break;
            case "Rook":
                if (PlayRook(chessPiece)) {
                    return true;
                }
                break;
            case "Bishop":
                if (PlayBishop(chessPiece)) {
                    return true;
                }
                break;
            case "Queen":
                if (PlayQueen(chessPiece)) {
                    return true;
                }
                break;
            case "Knight":
                if (PlayKnight(chessPiece)) {
                    return true;
                }
                break;
        }
        return false;
    }
    private bool hitted;
    private float lastTime;
    private bool playerisWh;
    private void ResetPlayerIsWh(bool isWh) {
        playerisWh = isWh;
    }
    private void Update() {
        if (UIManager.UIM.gameStart && PawnPromoteManager.isPromoting == false && ChessManager.gameOver == false && ChessManager.AIPlaying == false) {//如果不在观测视角
            if (ChessManager.AIZHIZHANG) {
                playerisWh ^= true;
                ChessManager.CM.isWhitePlayer = playerisWh;
                AIShowTime();
                return;
            }
            if (Input.GetButtonDown("Fire1")) {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hitted = false;
                hitted = Physics.Raycast(ray, out hit, Mathf.Infinity, layerMaskHighlightMesh | layerMaskPlayer);
                FirstHitJudge();
            }
            if (hasSelectedMesh && Input.GetButtonUp("Fire1")) {
                SecondHitJudge();
            }
        }
    }//通过鼠标控制棋子移动，与各种特效
    private void FirstHitJudge() {
        if (hitted && hit.collider.gameObject.layer == LayerMask.NameToLayer("HighlightBackground")) { // 高亮→选择
            selectedMeshGO = hit.collider.gameObject;
            // print("shootMesh!" + selectedMeshGO.transform.position);
            selectedMeshGO.SendMessage("OnSelectedMaterial");
            foreach (Vector3 element in targetPositionLinkedList) { //确定 highlightPosition
                if (chessboardDictionary[element] == selectedMeshGO)
                    selectedMeshPosition = element;
            }
            hasSelectedMesh = true;
        } else if (hitted && hit.collider.gameObject.layer == LayerMask.NameToLayer("Player")) { // 高亮Move范围
            if (chessPiece == hit.collider.gameObject && hasHighlightMesh) { // 重复选择同一个游戏对象就取消高亮，返回
                CancelHighlightMesh();
                chessPiece.SendMessage("OnParticleSystem", false);
                Camera.main.SendMessage("OnRollingView", true);
                if (chessPiece.tag == "King") //其实是只有自己的king才能draw
                    chessPiece.SendMessage("OnDraw");
                else {
                    AudioManager.AM.SendMessage("PlayUnChoose");
                }
                chessPiece = null;
                return;
            }
            GameObject chessPieceNew = hit.collider.gameObject; //开始处理新的选中对象
                                                                //print("shoot" + chessPiece.tag);
            bool isWhite = chessPieceNew.GetComponent<ChessPieceInfo>().isWhite;
            if (isWhite == ChessManager.CM.isWhiteTurn) {//得到自己的新棋子才能走
                if (hasHighlightMesh) //选择其他角色的情况下,只限自己，先清除上一个棋子状态 //如果有高亮的就取消高亮//点了其他人高亮纯属误点
                    CancelHighlightMesh();
                if (chessPiece != null) //取消粒子系统
                    chessPiece.SendMessage("OnParticleSystem", false);
                chessPiece = chessPieceNew;
                Camera.main.SendMessage("ChangeTarget", chessPiece);//点自己的子再切换视角
                if (SwitchPlayer(chessPiece.tag)) { //有路可走
                    HighlightMesh();
                    chessPiece.SendMessage("OnParticleSystem", true);
                    if (WaveFX) {
                        startMesh.SendMessage("StartWave", chessPiece.tag);
                    }
                    AudioManager.AM.SendMessage("PlayChoose");
                } else {
                    AudioManager.AM.SendMessage("PlayCantMove");
                }
                if (chessPiece.tag == "King")
                    chessPiece.SendMessage("OnDraw");
            }
        } else if (hasHighlightMesh && Camera.main.GetComponent<CameraFollow>().mouseMode == MouseMode.Follow) {//前次已经高亮 再次点击空白处取消高亮
            CancelHighlightMesh();
            chessPiece.SendMessage("OnParticleSystem", false);
            AudioManager.AM.SendMessage("PlayUnChoose");
        }
    }
    private void SecondHitJudge() {
        hasSelectedMesh = false;//先取消标记
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMaskSelectedMesh)) { // 选中selectedMesh，确定Move
            int musicIndex = 0;//默认只是移动
            CancelHighlightMesh();//移动先取消高亮
            chessPiece.SendMessage("OnParticleSystem", false);
            if (chessPiecesOnChessboardDictionary.ContainsKey(selectedMeshPosition)) {//吃子
                GameObject dead = chessPiecesOnChessboardDictionary[selectedMeshPosition];
                bool deadIsWhite = dead.GetComponent<ChessPieceInfo>().isWhite;
                PushChessMoveInfo(new ChessMoveInfo(deadIsWhite, dead, selectedMeshPosition, deadIsWhite ? Vector3.zero : -Vector3.one));//记录移子
                dead.SendMessage("DestroyToRest"); //触发DestroyToRest事件
                chessPiecesOnChessboardDictionary.Remove(selectedMeshPosition); //BUG 这个在DestroyToRest中
                if (WaveKillFX) {
                    selectedMeshGO.SendMessage("StartWave", "Boom");
                }
                musicIndex = 1;
            }
            PushChessMoveInfo(new ChessMoveInfo(chessPiece.GetComponent<ChessPieceInfo>().isWhite, chessPiece, chessPiece.GetComponent<ChessPieceInfo>().Position, selectedMeshPosition));//记录移子
            chessPiece.SendMessage("ChangePosition", selectedMeshPosition);//移子 触发ChangePosition事件
                                                                           //ChangePosition中可能触发pawn的升格事件，所以有 isPromotig
            if (PawnPromoteManager.isPromoting == false) {
                bool enemyIsWhite = !ChessManager.CM.isWhiteTurn;//移完子 看看check不check
                if (Check(enemyIsWhite)) {
                    if (Checkmate(enemyIsWhite)) {//对手无路可走
                        ChessManager.CM.SendMessage("OnCheckmate", enemyIsWhite);//给chessManager发送消息，进行UI处理
                        musicIndex = 3;
                    } else {
                        ChessManager.CM.SendMessage("OnCheck", enemyIsWhite);//发别的消息
                        musicIndex = 2;
                    }
                }
                if (ChessManager.AIMode) {//如果没有promote，那么不在这里交给AI，但是如果Promote，等promote结束再AI
                    AIShowTime();
                }
            }
            switch (musicIndex) {
                case 0:
                    AudioManager.AM.SendMessage("PlayMove");
                    break;
                case 1:
                    AudioManager.AM.SendMessage("PlayCapture");
                    break;
                case 2:
                    AudioManager.AM.SendMessage("PlayCheck");
                    break;
                case 3:
                    AudioManager.AM.SendMessage("PlayCheckmate");
                    break;
            }
            chessPiece = null;
            ChessManager.CM.SendMessage("ChangeTurn", true); // 转换回合 不管如何都会换回合的
        } else {//不移动 返回高亮
            if (chessPiecesOnChessboardDictionary.ContainsKey(selectedMeshPosition)) {
                if (MeshRendererManager.AttackMeshFX) {
                    selectedMeshGO.SendMessage("OnAttackMaterial");
                } else if (MeshRendererManager.HighlightMeshFX) {
                    selectedMeshGO.SendMessage("OnHighlightMaterial");
                } else {
                    selectedMeshGO.SendMessage("OnNormalMaterial");
                }
            } else {
                if (MeshRendererManager.HighlightMeshFX) {
                    selectedMeshGO.SendMessage("OnHighlightMaterial");
                } else {
                    selectedMeshGO.SendMessage("OnNormalMaterial");
                }
            }
            selectedMeshGO = null;
        }
        selectedMeshPosition = Vector3.zero;
    }

    private GameObject startMesh;
    private void CancelHighlightMesh() {
        foreach (Vector3 element in targetPositionLinkedList)
            chessboardDictionary[element].SendMessage("OnNormalMaterial");
        if (startMesh != null)
            startMesh.SendMessage("OnNormalMaterial");
        hasHighlightMesh = false;
    }
    private void HighlightMesh() {
        foreach (Vector3 element in targetPositionLinkedList) {
            if (chessPiecesOnChessboardDictionary.ContainsKey(element))
                chessboardDictionary[element].SendMessage("OnAttackMaterial");
            else
                chessboardDictionary[element].SendMessage("OnHighlightMaterial");
        }
        startMesh = chessboardDictionary[chessPiece.GetComponent<ChessPieceInfo>().Position];
        startMesh.SendMessage("OnStartMaterial");
        hasHighlightMesh = true;
    }

    private LinkedList<Vector3> targetPositionLinkedList = new LinkedList<Vector3>();//生成可走队列
    private Vector3 position;
    private Vector3 targetPosition;
    private void Add(Vector3 targetPosition, bool isCheck) {
        if (isCheck) {
            checkLinkedList.AddFirst(targetPosition);
        } else {
            targetPositionLinkedList.AddFirst(targetPosition);
        }
    }
    private bool PlayPawn(GameObject pawn, bool isCheck = false) {//isWhite pawn的势力
        if (isCheck)
            checkLinkedList.Clear();
        else {
            targetPositionLinkedList.Clear();
            chessPiece = pawn;
        }
        bool isWhite = pawn.GetComponent<ChessPieceInfo>().isWhite;
        position = pawn.GetComponent<ChessPieceInfo>().Position;
        targetPosition = position;
        if (isWhite) //白方回合
            targetPosition.y += 1;
        else //黑方回合
            targetPosition.y -= 1;
        if (!chessPiecesOnChessboardDictionary.ContainsKey(targetPosition))
            Add(targetPosition, isCheck); //直行加入链表，因为直行到底会触发变棋子，这个兵就不存在了
        targetPosition.x = position.x - 1;
        if (DetectOnlyEatEnemy(targetPosition, isWhite))
            Add(targetPosition, isCheck);
        targetPosition.x = position.x + 1;
        if (DetectOnlyEatEnemy(targetPosition, isWhite))
            Add(targetPosition, isCheck);
        targetPosition = position;
        targetPosition.z += 1;
        if (isWhite) {
            if (targetPosition.z > 0) {//斜前上方
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
            } else {
                targetPosition.y += 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
            }
        } else {
            if (targetPosition.z > 0) {//斜前上方
                targetPosition.y -= 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
            } else {
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
            }
        }
        targetPosition = position;
        targetPosition.z -= 1;
        if (isWhite) {
            if (targetPosition.z < 0) {
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
            } else {
                targetPosition.y += 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
            }
        } else {
            if (targetPosition.z < 0) {
                targetPosition.y -= 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
            } else {
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += 1;
                if (DetectOnlyEatEnemy(targetPosition, isWhite))
                    Add(targetPosition, isCheck);
            }
        }
        TestTargetPositionLinkedList(isWhite, isCheck);
        if (isCheck == false && targetPositionLinkedList.Count != 0)
            TestVirtualAdvanceCheck(isWhite);
        return targetPositionLinkedList.Count != 0;
    }
    private bool PlayKing(GameObject king, bool isCheck = false) {
        if (isCheck)
            checkLinkedList.Clear();
        else {
            targetPositionLinkedList.Clear();
            chessPiece = king;
        }
        bool isWhite = king.GetComponent<ChessPieceInfo>().isWhite;
        position = king.GetComponent<ChessPieceInfo>().Position;
        targetPosition = position;
        targetPosition.z = position.z + 1;
        if (position.z >= 0) { //斜上方
            Add(targetPosition, isCheck);
            targetPosition.x -= 1;
            Add(targetPosition, isCheck);
            targetPosition.y -= 1;
            Add(targetPosition, isCheck);
            targetPosition.x += 1;
            Add(targetPosition, isCheck);
        } else {
            Add(targetPosition, isCheck);
            targetPosition.x += 1;
            Add(targetPosition, isCheck);
            targetPosition.y += 1;
            Add(targetPosition, isCheck);
            targetPosition.x -= 1;
            Add(targetPosition, isCheck);
        }
        targetPosition = position;
        targetPosition.z = position.z - 1;
        if (position.z > 0) { //斜下方
            Add(targetPosition, isCheck);
            targetPosition.y += 1;
            Add(targetPosition, isCheck);
            targetPosition.x += 1;
            Add(targetPosition, isCheck);
            targetPosition.y -= 1;
            Add(targetPosition, isCheck);
        } else {
            Add(targetPosition, isCheck);
            targetPosition.y -= 1;
            Add(targetPosition, isCheck);
            targetPosition.x -= 1;
            Add(targetPosition, isCheck);
            targetPosition.y += 1;
            Add(targetPosition, isCheck);
        }
        targetPosition = position;//→←↑↓
        targetPosition.x += 1;
        Add(targetPosition, isCheck);
        targetPosition.x -= 2;
        Add(targetPosition, isCheck);
        targetPosition.x += 1;
        targetPosition.y += 1;
        Add(targetPosition, isCheck);
        targetPosition.y -= 2;
        Add(targetPosition, isCheck);
        TestTargetPositionLinkedList(isWhite, isCheck);
        if (isCheck == false && targetPositionLinkedList.Count != 0)
            TestVirtualAdvanceCheck(isWhite);
        return targetPositionLinkedList.Count != 0;
    }
    private bool PlayRook(GameObject rook, bool isCheck = false) {
        if (isCheck)
            checkLinkedList.Clear();
        else {
            targetPositionLinkedList.Clear();
            chessPiece = rook;
        }
        bool isWhite = rook.GetComponent<ChessPieceInfo>().isWhite;
        position = rook.GetComponent<ChessPieceInfo>().Position;
        float maxIndex = level - Mathf.Abs(position.z), num = 2;
        targetPosition = position;
        targetPosition.y += 1;
        while (targetPosition.y <= maxIndex) { //↑
            if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                Add(targetPosition, isCheck);
            targetPosition.y += 1;
        }
        num = 2;
        targetPosition = position;
        targetPosition.y -= 1;
        while (targetPosition.y > 0) { //↓
            if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                Add(targetPosition, isCheck);
            targetPosition.y -= 1;
        }
        num = 2;
        targetPosition = position;
        targetPosition.x += 1;
        while (targetPosition.x <= maxIndex) { // →
            if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                Add(targetPosition, isCheck);
            targetPosition.x += 1;
        }
        num = 2;
        targetPosition = position;
        targetPosition.x -= 1;
        while (targetPosition.x > 0) { // ←
            if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                Add(targetPosition, isCheck);
            targetPosition.x -= 1;
        }
        num = 2;
        targetPosition = position;
        targetPosition.z += 2;
        while (targetPosition.z < 6) { // 上
            if (targetPosition.z <= 0) {
                targetPosition.x += 1;
                targetPosition.y += 1;
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.z += 2;
            } else {
                targetPosition.x -= 1;
                targetPosition.y -= 1;
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.z += 2;
            }
        }
        num = 2;
        targetPosition = position;
        targetPosition.z -= 2;
        while (targetPosition.z > -6) { // 下
            if (targetPosition.z >= 0) {
                targetPosition.x += 1;
                targetPosition.y += 1;
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.z -= 2;
            } else {
                targetPosition.x -= 1;
                targetPosition.y -= 1;
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.z -= 2;
            }
        }
        TestTargetPositionLinkedList(isWhite, isCheck);
        if (isCheck == false && targetPositionLinkedList.Count != 0)
            TestVirtualAdvanceCheck(isWhite);
        return targetPositionLinkedList.Count != 0;
    }
    private bool PlayBishop(GameObject bishop, bool isCheck = false) {
        if (isCheck)
            checkLinkedList.Clear();
        else {
            targetPositionLinkedList.Clear();
            chessPiece = bishop;
        }
        bool isWhite = bishop.GetComponent<ChessPieceInfo>().isWhite;
        position = bishop.GetComponent<ChessPieceInfo>().Position;
        targetPosition = position;
        int offset = 1;
        float num = 2 * 3 * 5 * 7;
        targetPosition.z += 1;
        while (targetPosition.z < level) {//向上走
            if (targetPosition.z <= 0) {
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += offset;
                if (DetectInfiniteLine(ref num, 3, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y += offset;
                if (DetectInfiniteLine(ref num, 5, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= offset;
                if (DetectInfiniteLine(ref num, 7, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y -= offset;
            } else {
                targetPosition.x -= 1;
                targetPosition.y -= 1;
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += offset;
                if (DetectInfiniteLine(ref num, 3, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y += offset;
                if (DetectInfiniteLine(ref num, 5, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= offset;
                if (DetectInfiniteLine(ref num, 7, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y -= offset;
            }
            ++offset;
            targetPosition.z += 1;
        }
        targetPosition = position;
        offset = 1;
        num = 2 * 3 * 5 * 7;
        targetPosition.z -= 1;
        while (targetPosition.z > -level) { // 向下走
            if (targetPosition.z >= 0) {
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += offset;
                if (DetectInfiniteLine(ref num, 3, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y += offset;
                if (DetectInfiniteLine(ref num, 5, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= offset;
                if (DetectInfiniteLine(ref num, 7, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y -= offset;
            } else {
                targetPosition.x -= 1;
                targetPosition.y -= 1;
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += offset;
                if (DetectInfiniteLine(ref num, 3, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y += offset;
                if (DetectInfiniteLine(ref num, 5, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= offset;
                if (DetectInfiniteLine(ref num, 7, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y -= offset;
            }
            ++offset;
            targetPosition.z -= 1;
        }
        TestTargetPositionLinkedList(isWhite, isCheck);
        if (isCheck == false && targetPositionLinkedList.Count != 0)
            TestVirtualAdvanceCheck(isWhite);
        return targetPositionLinkedList.Count != 0;
    }
    private bool PlayQueen(GameObject queen, bool isCheck = false) {
        if (isCheck)
            checkLinkedList.Clear();
        else {
            targetPositionLinkedList.Clear();
            chessPiece = queen;
        }
        bool isWhite = queen.GetComponent<ChessPieceInfo>().isWhite;
        position = queen.GetComponent<ChessPieceInfo>().Position;
        float maxIndex = level - Mathf.Abs(position.z), num = 2;
        targetPosition = position;
        targetPosition.y += 1;
        while (targetPosition.y <= maxIndex) { //↑
            if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                Add(targetPosition, isCheck);
            targetPosition.y += 1;
        }
        num = 2;
        targetPosition = position;
        targetPosition.y -= 1;
        while (targetPosition.y > 0) { //↓
            if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                Add(targetPosition, isCheck);
            targetPosition.y -= 1;
        }
        num = 2;
        targetPosition = position;
        targetPosition.x += 1;
        while (targetPosition.x <= maxIndex) { // →
            if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                Add(targetPosition, isCheck);
            targetPosition.x += 1;
        }
        num = 2;
        targetPosition = position;
        targetPosition.x -= 1;
        while (targetPosition.x > 0) { // ←
            if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                Add(targetPosition, isCheck);
            targetPosition.x -= 1;
        }
        targetPosition = position;
        int offset = 1;
        num = 2 * 3 * 5 * 7;
        targetPosition.z += 1;
        while (targetPosition.z < level) {//向上走
            if (targetPosition.z <= 0) {
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += offset;
                if (DetectInfiniteLine(ref num, 3, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y += offset;
                if (DetectInfiniteLine(ref num, 5, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= offset;
                if (DetectInfiniteLine(ref num, 7, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y -= offset;
            } else {
                targetPosition.x -= 1;
                targetPosition.y -= 1;
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += offset;
                if (DetectInfiniteLine(ref num, 3, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y += offset;
                if (DetectInfiniteLine(ref num, 5, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= offset;
                if (DetectInfiniteLine(ref num, 7, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y -= offset;
            }
            ++offset;
            targetPosition.z += 1;
        }
        targetPosition = position;
        offset = 1;
        num = 2 * 3 * 5 * 7;
        targetPosition.z -= 1;
        while (targetPosition.z > -level) { // 向下走
            if (targetPosition.z >= 0) {
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += offset;
                if (DetectInfiniteLine(ref num, 3, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y += offset;
                if (DetectInfiniteLine(ref num, 5, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= offset;
                if (DetectInfiniteLine(ref num, 7, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y -= offset;
            } else {
                targetPosition.x -= 1;
                targetPosition.y -= 1;
                if (DetectInfiniteLine(ref num, 2, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x += offset;
                if (DetectInfiniteLine(ref num, 3, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y += offset;
                if (DetectInfiniteLine(ref num, 5, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.x -= offset;
                if (DetectInfiniteLine(ref num, 7, targetPosition, isWhite))
                    Add(targetPosition, isCheck);
                targetPosition.y -= offset;
            }
            ++offset;
            targetPosition.z -= 1;
        }
        TestTargetPositionLinkedList(isWhite, isCheck);
        if (isCheck == false && targetPositionLinkedList.Count != 0)
            TestVirtualAdvanceCheck(isWhite);
        return targetPositionLinkedList.Count != 0;
    }
    private bool PlayKnight(GameObject knight, bool isCheck = false) {
        if (isCheck)
            checkLinkedList.Clear();
        else {
            targetPositionLinkedList.Clear();
            chessPiece = knight;
        }
        bool isWhite = knight.GetComponent<ChessPieceInfo>().isWhite;
        position = knight.GetComponent<ChessPieceInfo>().Position;
        targetPosition = position; // 向上跳
        int num = 1;
        if (targetPosition.z < 0)
            num = -1;
        targetPosition.z += 1;
        targetPosition.x += 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.y -= 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.x -= 3 * num;
        Add(targetPosition, isCheck);
        targetPosition.y += 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.x += 1 * num;
        targetPosition.y += 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.x += 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.y -= 3 * num;
        Add(targetPosition, isCheck);
        targetPosition.x -= 1 * num;
        Add(targetPosition, isCheck);
        targetPosition = position; // 往下跳
        num = 1;
        if (targetPosition.z <= 0)
            num = -1;
        targetPosition.z -= 1;
        targetPosition.x -= 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.y += 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.x += 3 * num;
        Add(targetPosition, isCheck);
        targetPosition.y -= 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.x -= 1 * num;
        targetPosition.y -= 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.x -= 1 * num;
        Add(targetPosition, isCheck);
        targetPosition.y += 3 * num;
        Add(targetPosition, isCheck);
        targetPosition.x += 1 * num;
        Add(targetPosition, isCheck);
        TestTargetPositionLinkedList(isWhite, isCheck);
        if (isCheck == false && targetPositionLinkedList.Count != 0)
            TestVirtualAdvanceCheck(isWhite);
        return targetPositionLinkedList.Count != 0;
    }

    private GameObject targetPositionGameObject;//从可走队列中排除 敌方友方出界
    private bool DetectJustAvoidFriend(Vector3 targetPosition, bool isWhite) { //目标位置无人或者是敌军为true, isWhite友军
        if (chessPiecesOnChessboardDictionary.ContainsKey(targetPosition)) {
            targetPositionGameObject = chessPiecesOnChessboardDictionary[targetPosition];
            if (targetPositionGameObject.GetComponent<ChessPieceInfo>().isWhite == isWhite) {//遇到了友军就return false
                return false;
            }
        }
        return true;
    }
    private bool DetectOnlyEatEnemy(Vector3 targetPosition, bool isWhite) { //目标位置是敌军为true ,isWhite友军
        if (chessPiecesOnChessboardDictionary.ContainsKey(targetPosition)) {
            targetPositionGameObject = chessPiecesOnChessboardDictionary[targetPosition];
            if (targetPositionGameObject.GetComponent<ChessPieceInfo>().isWhite != isWhite) {//遇到了敌军就return true
                return true;
            }
        }
        return false;
    }
    private bool DetectInfiniteLine(ref float num, float keyNum, Vector3 targetPosition, bool isWhite) { //通过质数确认此路是否可通.isWhite友军
        if (num % keyNum == 0 && DetectJustAvoidFriend(targetPosition, isWhite)) {
            if (DetectOnlyEatEnemy(targetPosition, isWhite))
                num /= keyNum;
            return true;
        } else if (num % keyNum == 0)
            num /= keyNum;
        return false;
    }
    private void TestTargetPositionLinkedList(bool isWhite, bool isCheck = false) { //isWhite友方，排除LinkedList中出界的 与 可能触发Check isWhite友方的
        float maxIndex;
        Vector3[] positionArray;
        if (isCheck) {
            positionArray = new Vector3[checkLinkedList.Count];
            checkLinkedList.CopyTo(positionArray, 0);
        } else {
            positionArray = new Vector3[targetPositionLinkedList.Count];
            targetPositionLinkedList.CopyTo(positionArray, 0);
        }
        for (int i = positionArray.Length - 1; i > -1; --i) {
            maxIndex = level - Mathf.Abs(positionArray[i].z);
            if (positionArray[i].x < 1 || positionArray[i].x > maxIndex || positionArray[i].y < 1 || positionArray[i].y > maxIndex) {//出界排除
                if (isCheck)
                    checkLinkedList.Remove(positionArray[i]);
                else
                    targetPositionLinkedList.Remove(positionArray[i]);
            }
            if (!DetectJustAvoidFriend(positionArray[i], isWhite)) {//防友军移除
                if (isCheck)
                    checkLinkedList.Remove(positionArray[i]);
                else
                    targetPositionLinkedList.Remove(positionArray[i]);
            }
        }
    }

    private LinkedList<Vector3> checkLinkedList = new LinkedList<Vector3>();//用于判断是否 Check Or Checkmate Enemy
    private void TestVirtualAdvanceCheck(bool isWhite) {//isWhite友军
        if (RemoveStillCheckMeshFX == false) {
            return;
        }
        Vector3[] positionArray = new Vector3[targetPositionLinkedList.Count];// 现在已经有targetPositionLinkedList中的位置了
        targetPositionLinkedList.CopyTo(positionArray, 0);
        for (int i = positionArray.Length - 1; i > -1; --i) { //虚拟遍历所有位置
            Vector3 originalPosition = chessPiece.GetComponent<ChessPieceInfo>().Position;//得到移动棋子的起始坐标
            chessPiecesOnChessboardDictionary.Remove(originalPosition);//抬脚，改变chessPiecesOnChessboardDictionary
            bool hasEnemy = false;
            GameObject enemy = null;
            int indexEnemy = isWhite ? 1 : 0;//敌人坐标                                                                    
            if (chessPiecesOnChessboardDictionary.ContainsKey(positionArray[i])) {//考虑目标位置为敌军 改变chessPiecesLists
                hasEnemy = true;
                enemy = chessPiecesOnChessboardDictionary[positionArray[i]];
                chessPiecesLists[indexEnemy].Remove(enemy);
                chessPiecesOnChessboardDictionary.Remove(positionArray[i]);
            }
            chessPiecesOnChessboardDictionary.Add(positionArray[i], chessPiece);//落脚
            chessPiece.GetComponent<ChessPieceInfo>().Position = positionArray[i];
            if (Check(isWhite)) {//Check our King，如果为true，就删
                targetPositionLinkedList.Remove(positionArray[i]);
            }
            chessPiecesOnChessboardDictionary.Remove(positionArray[i]);//不管如何 改回来
            chessPiecesOnChessboardDictionary.Add(originalPosition, chessPiece);
            chessPiece.GetComponent<ChessPieceInfo>().Position = originalPosition;
            if (hasEnemy) {
                chessPiecesLists[indexEnemy].Add(enemy);
                chessPiecesOnChessboardDictionary.Add(positionArray[i], enemy);
            }
        }
    }
    private bool Check(bool enemyIsWhite) { //enemyIswhite，被Check的king //如果成功check，就返回true
                                            //得到对方king的位置
        Vector3 enemyKingPosition = Vector3.zero;
        int indexEnemyIsWhite = 0;
        if (enemyIsWhite == false)
            indexEnemyIsWhite = 1;
        foreach (GameObject isKing in chessPiecesLists[indexEnemyIsWhite]) {
            if (isKing.tag == "King")
                enemyKingPosition = isKing.GetComponent<ChessPieceInfo>().Position;
        }
        //得到所有自己的子
        int indexWalewale = 0;
        if (indexEnemyIsWhite == 0)
            indexWalewale = 1;
        foreach (GameObject element in chessPiecesLists[indexWalewale]) {// 这里是isCheck的万恶之源
                                                                         //判断所有自己的子的攻击范围，如果有对方king的位置，就check
            switch (element.tag) {
                case "Pawn":
                    if (PlayPawn(element, true) && checkLinkedList.Contains(enemyKingPosition)) {
                        return true;
                    }
                    break;
                case "King":
                    if (PlayKing(element, true) && checkLinkedList.Contains(enemyKingPosition)) {
                        return true;
                    }
                    break;
                case "Rook":
                    if (PlayRook(element, true) && checkLinkedList.Contains(enemyKingPosition)) {
                        return true;
                    }
                    break;
                case "Bishop":
                    if (PlayBishop(element, true) && checkLinkedList.Contains(enemyKingPosition)) {
                        return true;
                    }
                    break;
                case "Queen":
                    if (PlayQueen(element, true) && checkLinkedList.Contains(enemyKingPosition)) {
                        return true;
                    }
                    break;
                case "Knight":
                    if (PlayKnight(element, true) && checkLinkedList.Contains(enemyKingPosition)) {
                        return true;
                    }
                    break;
            }
        }
        return false;
    }
    private bool Checkmate(bool enemyIsWhite) {// enemyIsWhite 被checkmate的一方 没得走了就输了呗
        foreach (GameObject element in chessPiecesLists[enemyIsWhite ? 0 : 1]) {//遍历所有的子
            switch (element.tag) {
                case "Pawn":
                    if (PlayPawn(element)) {//有路可走就还没输
                        return false;
                    }
                    break;
                case "King":
                    if (PlayKing(element)) {
                        return false;
                    }
                    break;
                case "Rook":
                    if (PlayRook(element)) {
                        return false;
                    }
                    break;
                case "Bishop":
                    if (PlayBishop(element)) {
                        return false;
                    }
                    break;
                case "Queen":
                    if (PlayQueen(element)) {
                        return false;
                    }
                    break;
                case "Knight":
                    if (PlayKnight(element)) {
                        return false;
                    }
                    break;
            }
        }
        return true;//checkmate
    }
    private void AfterMoveCheck(bool enemyIsWhite) {//bug,卒的升格之后悔棋再升格不会再次Check，我要崩溃了
        if (Check(enemyIsWhite)) {
            if (Checkmate(enemyIsWhite)) {//对手无路可走
                ChessManager.CM.SendMessage("OnCheckmate", enemyIsWhite);//给chessManager发送消息，进行UI处理
            } else {
                if (ChessManager.CheckUI) {
                    ChessManager.CM.SendMessage("OnCheck", enemyIsWhite);//发别的消息
                }
                if (AudioManager.CheckSound) {
                    AudioManager.AM.SendMessage("PlayCheck");
                }
            }
        }
    }// 令人头疼的CheckOrCheckmate

    private LinkedList<ChessMoveInfo> chessMoveInfoLinkedList = new LinkedList<ChessMoveInfo>();//悔棋队列
    private void PushChessMoveInfo(ChessMoveInfo info) {
        chessMoveInfoLinkedList.AddLast(info);
    }
    public bool PopChessMoveInfo() {//悔棋
        if (chessMoveInfoLinkedList.Count == 0) {
            return false;
        }
        ShutDown();
        ChessMoveInfo info = chessMoveInfoLinkedList.Last.Value;//第一个最后一个Move
        chessMoveInfoLinkedList.RemoveLast();//先删了
        if ((info as ChessPromoteInfo) == null) {//如果不是pawn的升格，那么就是普通移动的一步
            info.chessPiece.SendMessage("ChangePosition", info.from);//后入队列先处理，处理主动棋子
            if (chessMoveInfoLinkedList.Count != 0) {//如果不是第一步，那么last存在
                if (chessMoveInfoLinkedList.Last.Value.to == -Vector3.one || chessMoveInfoLinkedList.Last.Value.to == Vector3.zero) { //如果上一步有子被吃，那么就要操作两个人
                    ChessMoveInfo info2 = chessMoveInfoLinkedList.Last.Value;//这是dead
                    chessMoveInfoLinkedList.RemoveLast();
                    DeadListManager.DLM.SendMessage("Remove", info2.chessPiece);//死亡队列除名
                    chessPiecesLists[info2.isWhite ? 0 : 1].Add(info2.chessPiece);//复活归队
                    chessPiecesOnChessboardDictionary.Add(info2.from, info2.chessPiece);//复活回归棋盘
                    info2.chessPiece.GetComponent<ChessPieceInfo>().Position = info2.from;
                    info2.chessPiece.SendMessage("SetTargetPosition", chessboardDictionary[info2.from].transform.position);
                }
            }
            ChessManager.CM.isWhiteTurn = info.isWhite;//回归回合
            ChessManager.CM.SendMessage("ReturnRound");
        } else {//玩家 AI升格
            ChessPromoteInfo pInfo = info as ChessPromoteInfo;//只是变子回合是不会变的
            PawnPromoteManager.ChangeAppearance(pInfo.chessPiece, pawnModelNew, pInfo.fromTag);
            if (UIManager.UIM.gameStart) {//如果游戏没有开始，就无视掉升格的操作，要不然还有摄像机移动
                if (ChessManager.AIMode == false) {
                    PawnPromoteManager.PPM.SendMessage("StartPromotion", pInfo.chessPiece);
                } else {//AI模式下
                    if (pInfo.isWhite == ChessManager.CM.isWhitePlayer) {//玩家的子升格
                        PawnPromoteManager.PPM.SendMessage("StartPromotion", pInfo.chessPiece);
                    } else {//电脑的子升格
                        PawnPromoteManager.ChangeAppearance(pInfo.chessPiece, pawnModelNew, "Pawn");//直接变成Pawn
                        PopChessMoveInfo();//再来一遍，一定不是升格的情况，要不会无限循环
                        ChessManager.CM.SendMessage("ReturnRound");
                    }
                }
            }
        }
        return true;
    }

    private int _maxScore = 30000;
    private int _depth;
    private ChessMoveInfo _chessMoveInfo;
    private int pawnSearchPoint;
    private int rookSearchPoint;
    private int bishopSearchPoint;
    private int kingSearchPoint;
    private int queenSearchPoint;
    private int knightSearchPoint;
    private string currentFirstDepthPieceTag;
    private string lastFirstDepthPieceTag;
    private int plusPoint;
    public int AddablePawnSearchPoint { get; private set; }//属性就属性吧。。
    public int AddableRookSearchPoint { get; private set; }
    public int AddableBishopSearchPoint { get; private set; }
    public int AddableKingSearchPoint { get; private set; }
    public int AddableQueenSearchPoint { get; private set; }
    public int AddableKnightSearchPoint { get; private set; }

    private void AIShowTime() {//AlphaBeta剪枝 
        ChessManager.CM.SendMessage("SetAIDifficulty");
        // AIResetSearchPoint(); //重新准备设置下一轮sort所需要的search参数
        ChessManager.AIPlaying = true;
        bool changeCheck = false;
        if (RemoveStillCheckMeshFX == false) {
            RemoveStillCheckMeshFX = true;
            changeCheck = true;
        }
        //减少性能消耗
        _depth = ChessManager.AIMaxDepth;
        _chessMoveInfo = null;
        int finalScore = AIMAX(ChessManager.AIMaxDepth, -_maxScore, _maxScore);
        if (Mathf.Abs(finalScore) > _maxScore) {
            print("BUG Score Evaluate exceed bundary");
        }
        //  AIAddToAddableSearchPoint();//更新下一轮所需要的参数为了Sort Search
        int musicIndex = 0;//默认只是移动
        if (_chessMoveInfo != null) {//没有Checkmate
            if (chessPiecesOnChessboardDictionary.ContainsKey(_chessMoveInfo.to)) {//吃子
                GameObject dead = chessPiecesOnChessboardDictionary[_chessMoveInfo.to];
                PushChessMoveInfo(new ChessMoveInfo(!_chessMoveInfo.isWhite, dead, _chessMoveInfo.to, !_chessMoveInfo.isWhite ? Vector3.zero : -Vector3.one));//记录移子
                dead.SendMessage("DestroyToRest"); //触发DestroyToRest事件
                chessPiecesOnChessboardDictionary.Remove(_chessMoveInfo.to); //BUG 这个在DestroyToRest中
                if (WaveKillFX) {
                    if (ChessManager.AIZHIZHANG == false) {
                        chessboardDictionary[_chessMoveInfo.to].SendMessage("StartWave", "Boom");
                    }
                }
                musicIndex = 1;
            }
            PushChessMoveInfo(_chessMoveInfo);//记录移子
            _chessMoveInfo.chessPiece.SendMessage("ChangePosition", _chessMoveInfo.to);//移子 触发ChangePosition事件
            if (Check(ChessManager.CM.isWhitePlayer)) {
                if (Checkmate(ChessManager.CM.isWhitePlayer)) {//对手无路可走
                    ChessManager.CM.SendMessage("OnCheckmate", ChessManager.CM.isWhitePlayer);//给chessManager发送消息，进行UI处理
                    musicIndex = 3;
                } else {
                    if (ChessManager.CheckUI) {
                        ChessManager.CM.SendMessage("OnCheck", ChessManager.CM.isWhitePlayer);//发别的消息
                    }
                    musicIndex = 2;
                }
            }
        } else {//Checkmate
            if (finalScore == _maxScore) {
                ChessManager.CM.SendMessage("OnCheckmate", ChessManager.CM.isWhitePlayer);//给chessManager发送消息，进行UI处理，Player输了
            } else if (finalScore == -_maxScore) {
                ChessManager.CM.SendMessage("OnCheckmate", !ChessManager.CM.isWhitePlayer);//AI输了
            }
            musicIndex = 3;
        }
        switch (musicIndex) {
            case 0:
                AudioManager.AM.SendMessage("PlayMove");
                break;
            case 1:
                AudioManager.AM.SendMessage("PlayCapture");
                break;
            case 2:
                AudioManager.AM.SendMessage("PlayCheck");
                break;
            case 3:
                AudioManager.AM.SendMessage("PlayCheckmate");
                break;
        }
        //恢复性能消耗
        if (changeCheck) {
            RemoveStillCheckMeshFX = false;
        }
        ChessManager.AIPlaying = false;
        ChessManager.CM.SendMessage("ChangeTurn", true); // 转换回合
    }
    private int AIMAX(int depth, int alpha, int beta) { //AI走一步
        //print("Depth==" + depth + " Max");
        if (depth == 0) {
            //AISetSearchPoint(currentFirstDepthPieceTag);
            return AIEvaluate();
        } else {
            if (Checkmate(!ChessManager.CM.isWhitePlayer)) {//MAX只能是Player Checkmate AI，所以Checkmate（！Player）
                return -_maxScore;//AI被check 完了
            }
        }
        List<ChessMoveInfo> moveList = new List<ChessMoveInfo>();
        AIGenerateMoves(moveList, !ChessManager.CM.isWhitePlayer);//Max函数中生成的是玩家的步骤
        while (moveList.Count != 0) {//还有可以走的
            ChessMoveInfo move = moveList[0];
            //if (depth == _depth) {
            //    currentFirstDepthPieceTag = move.chessPiece.tag;
            //    if (currentFirstDepthPieceTag != lastFirstDepthPieceTag) {
            //        ++plusPoint;
            //        lastFirstDepthPieceTag = currentFirstDepthPieceTag;
            //    }
            //}
            AIMove(move);
            int score = AIMIN(depth - 1, alpha, beta);
            //print("Depth==" + depth + " Max");
            if (AIUnMove())
                AIUnMove();
            if (score >= beta) {
                return score;
            }
            if (score > alpha) {
                alpha = score;
                if (depth == _depth) {
                    _chessMoveInfo = move;
                }
            }
            moveList.RemoveAt(0);
        }
        return alpha;
    }
    private int AIMIN(int depth, int alpha, int beta) { //Player走一步
                                                        //print("Depth==" + depth + " Min");
        if (depth == 0) {
            //  AISetSearchPoint(currentFirstDepthPieceTag);
            return AIEvaluate();
        } else {
            if (Checkmate(ChessManager.CM.isWhitePlayer)) {//MIN只能是AI Checkmate Player，所以Checkmate（Player）
                return _maxScore;//PLayer被check Oyea
            }
        }
        List<ChessMoveInfo> moveList = new List<ChessMoveInfo>();
        AIGenerateMoves(moveList, ChessManager.CM.isWhitePlayer);//Min函数中生成的是玩家的步骤
        while (moveList.Count != 0) {//还有可以走的
            ChessMoveInfo move = moveList[0];
            //if (depth == _depth) {
            //    currentFirstDepthPieceTag = move.chessPiece.tag;
            //    if (currentFirstDepthPieceTag != lastFirstDepthPieceTag) {
            //        ++plusPoint;
            //        lastFirstDepthPieceTag = currentFirstDepthPieceTag;
            //    }
            //}
            AIMove(move);
            int score = AIMAX(depth - 1, alpha, beta);
            // print("Depth==" + depth + " Min");
            if (AIUnMove())
                AIUnMove();
            if (score <= alpha) {
                return score;
            }
            if (score < beta) {
                beta = score;
                if (depth == _depth) {
                    _chessMoveInfo = move;
                }
            }
            moveList.RemoveAt(0);
        }
        return beta;
    }
    private void AIGenerateMoves(List<ChessMoveInfo> list, bool isWhite) { //生成所有move
                                                                           //棋子排序 ， 位置排序
        foreach (GameObject element in chessPiecesLists[isWhite ? 0 : 1]) {
            switch (element.tag) {
                case "Pawn":
                    if (PlayPawn(element)) {
                        Vector3 startPosition = element.GetComponent<ChessPieceInfo>().Position;
                        //print("Pawn" + "Start " + startPosition + " END" + targetPosition);
                        foreach (Vector3 targetPosition in targetPositionLinkedList) {
                            list.Add(new ChessMoveInfo(isWhite, element, startPosition, targetPosition));
                        }
                    }
                    break;
                case "King":
                    if (PlayKing(element)) {
                        Vector3 startPosition = element.GetComponent<ChessPieceInfo>().Position;
                        //print("King" + "Start " + startPosition + " END" + targetPosition);
                        foreach (Vector3 targetPosition in targetPositionLinkedList) {
                            list.Add(new ChessMoveInfo(isWhite, element, startPosition, targetPosition));
                        }
                    }
                    break;
                case "Bishop":
                    if (PlayBishop(element)) {
                        Vector3 startPosition = element.GetComponent<ChessPieceInfo>().Position;
                        //print("Bishop" + "Start " + startPosition + " END" + targetPosition);
                        foreach (Vector3 targetPosition in targetPositionLinkedList) {
                            list.Add(new ChessMoveInfo(isWhite, element, startPosition, targetPosition));
                        }
                    }
                    break;
                case "Rook":
                    if (PlayRook(element)) {
                        Vector3 startPosition = element.GetComponent<ChessPieceInfo>().Position;
                        //print("Rook" + "Start " + startPosition + " END" + targetPosition);
                        foreach (Vector3 targetPosition in targetPositionLinkedList) {
                            list.Add(new ChessMoveInfo(isWhite, element, startPosition, targetPosition));
                        }
                    }
                    break;
                case "Queen":
                    if (PlayQueen(element)) {
                        Vector3 startPosition = element.GetComponent<ChessPieceInfo>().Position;
                        //print("Queen" + "Start " + startPosition + " END" + targetPosition);
                        foreach (Vector3 targetPosition in targetPositionLinkedList) {
                            list.Add(new ChessMoveInfo(isWhite, element, startPosition, targetPosition));
                        }
                    }
                    break;
                case "Knight":
                    if (PlayKnight(element)) {
                        Vector3 startPosition = element.GetComponent<ChessPieceInfo>().Position;
                        //print("Knight" + "Start " + startPosition + " END" + targetPosition);
                        foreach (Vector3 targetPosition in targetPositionLinkedList) {
                            list.Add(new ChessMoveInfo(isWhite, element, startPosition, targetPosition));
                        }
                    }
                    break;
            }
        }
        list.Sort((x, y) => -AISortCompare(x.chessPiece, y.chessPiece));
    }
    private void AIMove(ChessMoveInfo move) { //改变一下棋盘数据
                                              // 考虑普通移动/吃子/升格
        Vector3 toPosition = move.to;
        if (chessPiecesOnChessboardDictionary.ContainsKey(toPosition)) {//吃子
            GameObject dead = chessPiecesOnChessboardDictionary[toPosition];
            bool deadIsWhite = !move.isWhite;
            PushChessMoveInfo(new ChessMoveInfo(deadIsWhite, dead, toPosition, deadIsWhite ? Vector3.zero : -Vector3.one));//记录移子
                                                                                                                           //  print("KillingKilling"+dead.name + "From" + toPosition + "To" + (deadIsWhite ? Vector3.zero : -Vector3.one));
            chessPiecesLists[deadIsWhite ? 0 : 1].Remove(dead); //chessPiecesLists除名
            chessPiecesOnChessboardDictionary.Remove(toPosition);
        }
        chessPiecesOnChessboardDictionary.Remove(move.from);
        chessPiecesOnChessboardDictionary.Add(toPosition, move.chessPiece);
        move.chessPiece.GetComponent<ChessPieceInfo>().Position = toPosition;
        PushChessMoveInfo(move);//记录移子
                                //print("MovingMoving" + move.chessPiece.name + "From" + move.from + "To" + toPosition);
        if (move.chessPiece.tag == "Pawn") {//如果升格
            if (move.isWhite) {
                if (toPosition.y == level - (int)Mathf.Abs(toPosition.z)) {
                    move.chessPiece.tag = "Queen";
                    ChessPromoteInfo chessPromoteInfo = new ChessPromoteInfo(move.isWhite, move.chessPiece, toPosition, toPosition, "Pawn", "Queen");
                    PushChessMoveInfo(chessPromoteInfo);
                    // print("Promoting "+move.chessPiece.name+ "升格");
                }
            }
        }
    }
    public bool AIUnMove() { // 恢复棋盘数据 是否要再次Pop
                             // 回复现场，考虑移动/吃子/升格
        if (chessMoveInfoLinkedList.Count == 0) {
            return false;
        }
        ChessMoveInfo info = chessMoveInfoLinkedList.Last.Value;//第一个最后一个Move
        chessMoveInfoLinkedList.RemoveLast();//先删了
        if ((info as ChessPromoteInfo) == null) {//如果不是pawn的升格，那么就是普通移动的一步
            if (chessMoveInfoLinkedList.Count != 0) {//如果不是第一步，那么last存在
                Vector3 info2ToPosition = chessMoveInfoLinkedList.Last.Value.to;
                if (info2ToPosition == -Vector3.one || info2ToPosition == Vector3.zero) { //如果上一步有子被吃，那么就要操作两个人
                    ChessMoveInfo info2 = chessMoveInfoLinkedList.Last.Value;//这是dead
                    chessMoveInfoLinkedList.RemoveLast();
                    chessPiecesLists[info2.isWhite ? 0 : 1].Add(info2.chessPiece);//复活归队
                                                                                  //死亡棋子回归需要有一个先后顺序
                    chessPiecesOnChessboardDictionary.Remove(info.to);//普通移动，就会有子的位置变化
                    chessPiecesOnChessboardDictionary.Add(info.from, info.chessPiece);
                    info.chessPiece.GetComponent<ChessPieceInfo>().Position = info.from;

                    //print(info2.chessPiece.name + "DeadTo" + info2.from);
                    chessPiecesOnChessboardDictionary.Add(info2.from, info2.chessPiece);//复活回归棋盘
                } else {
                    chessPiecesOnChessboardDictionary.Remove(info.to);//普通移动，就会有子的位置变化
                    chessPiecesOnChessboardDictionary.Add(info.from, info.chessPiece);
                    info.chessPiece.GetComponent<ChessPieceInfo>().Position = info.from;
                }
            } else {
                chessPiecesOnChessboardDictionary.Remove(info.to);//普通移动，就会有子的位置变化
                chessPiecesOnChessboardDictionary.Add(info.from, info.chessPiece);
                info.chessPiece.GetComponent<ChessPieceInfo>().Position = info.from;
            }
        } else {
            info.chessPiece.tag = "Pawn";
            return true;
        }
        return false;
    }
    private int AIEvaluate() { //评估局面 对AI有利为正
        int score = 0;//只管加就行了
        score += AIEvalPiecePower(ChessManager.CM.isWhitePlayer);
        score += AIEvalPiecePower(!ChessManager.CM.isWhitePlayer);
        return score;
    }
    private int AIEvalPiecePower(bool isWhite) {
        int score = 0;
        int count = 0;
        foreach (GameObject element in chessPiecesLists[isWhite ? 0 : 1]) {
            ++count;
            switch (element.tag) {
                case "Pawn":
                    score += 200;
                    if (isWhite) {
                        Vector3 position = element.GetComponent<ChessPieceInfo>().Position;
                        int num = (int)Mathf.Abs(position.z) + (int)position.y - 1;
                        score += (int)(30 + (8 + 0.28f * (ChessManager.round - 1)) * num) * num;

                        position.y -= 1;//挡住罚分 
                        if (chessPiecesOnChessboardDictionary.ContainsKey(position)) {
                            string name = chessPiecesOnChessboardDictionary[position].name;
                            if (name == "Rook1W" || name == "Rook2W" || name == "QueenW") {
                                score -= 30;
                            }
                        }
                        position.y += 2;
                        if (chessPiecesOnChessboardDictionary.ContainsKey(position)) {
                            string name = chessPiecesOnChessboardDictionary[position].name;
                            if (name == "Rook1W" || name == "Rook2W" || name == "QueenW") {
                                score -= 30;
                            }
                        }
                        position.y -= 1;
                        position.x -= 1;
                        if (chessPiecesOnChessboardDictionary.ContainsKey(position)) {
                            string name = chessPiecesOnChessboardDictionary[position].name;
                            if (name == "Rook1W" || name == "Rook2W" || name == "QueenW") {
                                score -= 30;
                            }
                        }
                        position.x += 2;
                        if (chessPiecesOnChessboardDictionary.ContainsKey(position)) {
                            string name = chessPiecesOnChessboardDictionary[position].name;
                            if (name == "Rook1W" || name == "Rook2W" || name == "QueenW") {
                                score -= 30;
                            }
                        }
                    } else {
                        Vector3 position = element.GetComponent<ChessPieceInfo>().Position;
                        int maxIndex = level - (int)Mathf.Abs(position.z);
                        int num = (int)Mathf.Abs(position.z) + maxIndex - (int)position.y;
                        score += (int)(30 + (8 + 0.28f * (ChessManager.round - 1)) * num) * num;

                        position.y -= 1;
                        if (chessPiecesOnChessboardDictionary.ContainsKey(position)) {
                            string name = chessPiecesOnChessboardDictionary[position].name;
                            if (name == "Rook1B" || name == "Rook2B" || name == "QueenB") {
                                score -= 30;
                            }
                        }
                        position.y += 2;
                        if (chessPiecesOnChessboardDictionary.ContainsKey(position)) {
                            string name = chessPiecesOnChessboardDictionary[position].name;
                            if (name == "Rook1B" || name == "Rook2B" || name == "QueenB") {
                                score -= 30;
                            }
                        }
                        position.y -= 1;
                        position.x -= 1;
                        if (chessPiecesOnChessboardDictionary.ContainsKey(position)) {
                            string name = chessPiecesOnChessboardDictionary[position].name;
                            if (name == "Rook1B" || name == "Rook2B" || name == "QueenB") {
                                score -= 30;
                            }
                        }
                        position.x += 2;
                        if (chessPiecesOnChessboardDictionary.ContainsKey(position)) {
                            string name = chessPiecesOnChessboardDictionary[position].name;
                            if (name == "Rook1B" || name == "Rook2B" || name == "QueenB") {
                                score -= 30;
                            }
                        }
                    }
                    break;
                case "Bishop":
                    score += 450;
                    break;
                case "Rook":
                    score += 400;
                    if (ChessManager.round < 7) {
                        PlayRook(element);
                        score += 10 * (targetPositionLinkedList.Count - 3);
                    }
                    break;
                case "Queen":
                    score += 900;
                    break;
                case "Knight":
                    score += 375;
                    if (ChessManager.round < 7) {
                        if (isWhite) {
                            Vector3 position = element.GetComponent<ChessPieceInfo>().Position;
                            int num = ((int)position.y - 1);
                            if ((int)Mathf.Abs(position.z) <= 1) {
                                num += (int)Mathf.Abs(position.z);
                            }
                            score += 10 * num;
                        } else {
                            Vector3 position = element.GetComponent<ChessPieceInfo>().Position;
                            int maxIndex = level - (int)Mathf.Abs(position.z);
                            int num = maxIndex - (int)position.y;
                            if ((int)Mathf.Abs(position.z) <= 1) {
                                num += (int)Mathf.Abs(position.z);
                            }
                            score += 10 * num;
                        }
                    }
                    break;
                case "King":
                    if (ChessManager.round <= 100) {
                        score += 800;
                    } else {
                        score += 20000;
                    }
                    break;
            }
        }
        score += Random.Range(-30, 30);
        if (isWhite == ChessManager.CM.isWhitePlayer) {
            return -score;
        } else {
            return score;
        }
    }
    private int AISortCompare(GameObject x, GameObject y) {
        int result = AISortOrder(x.tag) - AISortOrder(y.tag);
        if (result > 0)
            return 1;
        else if (result < 0)
            return -1;
        else return 0;
    }
    private int AISortOrder(string tag) {//最先排序的数值最大
        int score = 0;
        if (ChessManager.round < 5) {
            switch (tag) {
                case "Pawn":
                    score += 10;
                    break;
                case "Rook":
                    score += 8;
                    break;
                case "Bishop":
                    score += 6;
                    break;
                case "King":
                    score += 5;
                    break;
                case "Queen":
                    score += 7;
                    break;
                case "Knight":
                    score += 9;
                    break;
            }
        } else if (ChessManager.round < 20) {
            switch (tag) {
                case "Pawn":
                    score += 6;
                    break;
                case "Rook":
                    score += 10;
                    break;
                case "Bishop":
                    score += 8;
                    break;
                case "King":
                    score += 5;
                    break;
                case "Queen":
                    score += 7;
                    break;
                case "Knight":
                    score += 9;
                    break;
            }
        } else if (ChessManager.round >= 20) {
            switch (tag) {
                case "Pawn":
                    score += 7;
                    break;
                case "Rook":
                    score += 8;
                    break;
                case "Bishop":
                    score += 9;
                    break;
                case "King":
                    score += 5;
                    break;
                case "Queen":
                    score += 10;
                    break;
                case "Knight":
                    score += 6;
                    break;
            }
        }
        switch (tag) {
            case "Pawn":
                score += AddablePawnSearchPoint;
                break;
            case "Rook":
                score += AddableRookSearchPoint;
                break;
            case "Bishop":
                score += AddableBishopSearchPoint;
                break;
            case "King":
                score += AddableKingSearchPoint;
                break;
            case "Queen":
                score += AddableQueenSearchPoint;
                break;
            case "Knight":
                score += AddableKnightSearchPoint;
                break;
        }
        return score;
    }
    private void AIResetSearchPoint() {
        pawnSearchPoint = 0;
        rookSearchPoint = 0;
        bishopSearchPoint = 0;
        kingSearchPoint = 0;
        queenSearchPoint = 0;
        knightSearchPoint = 0;
        plusPoint = 0;
        lastFirstDepthPieceTag = "";
    }
    private void AISetSearchPoint(string tag) {
        switch (tag) {
            case "Pawn":
                pawnSearchPoint += plusPoint;
                break;
            case "Rook":
                rookSearchPoint += plusPoint;
                break;
            case "Bishop":
                bishopSearchPoint += plusPoint;
                break;
            case "King":
                kingSearchPoint += plusPoint;
                break;
            case "Queen":
                queenSearchPoint += plusPoint;
                break;
            case "Knight":
                knightSearchPoint += plusPoint;
                break;
        }
    }
    private void AIAddToAddableSearchPoint() {
        AddablePawnSearchPoint = pawnSearchPoint;
        AddableRookSearchPoint = rookSearchPoint;
        AddableBishopSearchPoint = bishopSearchPoint;
        AddableQueenSearchPoint = queenSearchPoint;
        AddableKingSearchPoint = kingSearchPoint;
        AddableKnightSearchPoint = knightSearchPoint;
    }

    private void ThisChangeTarget(GameObject target) {
        ShutDown();
        chessPiece = target;
        if (SwitchPlayer(target.tag)) {
            HighlightMesh();
            chessPiece.SendMessage("OnParticleSystem", true);
        }
    }
    private void ShutDown() {
        hasSelectedMesh = false;
        if (selectedMeshGO != null)
            selectedMeshGO.SendMessage("OnNormalMaterial");
        selectedMeshPosition = Vector3.zero;
        CancelHighlightMesh();
        if (chessPiece != null) {//取消粒子系统
            chessPiece.SendMessage("OnParticleSystem", false);
            chessPiece = null;
        }
        targetPositionLinkedList.Clear();
        checkLinkedList.Clear();
    }
}