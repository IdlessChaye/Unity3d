using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ChessManager : MonoBehaviour {
    private static ChessManager gameManager;
    public static ChessManager CM {
        get {
            return gameManager;
        }
    }
    public bool isWhiteTurn;
    public bool isWhitePlayer;
    public static bool chessboardVisible;
    public static bool gameOver;
    public static int round;
    public static bool AIPlaying;
    public static bool AIMode;
    public static int AIMaxDepth;

    public static bool CheckUI { get; set; }
    public static int AIDifficulty { get; set; }
    public static bool AIZHIZHANG { get; set; }
    void SetBools() {
        CheckUI = true;
        AIZHIZHANG = false;
    }
    void SetInts() {
        AIDifficulty = 2;
    }
    private void Awake() {
        gameManager = this;
        chessboardVisible = true;
        isWhitePlayer = true;
        AIPlaying = false;
        round = 1;//默认第一局是round1
        gameOver = false;
        SetBools();
        SetInts();
        SetAIDifficulty();
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Z)) {
            if (Input.GetKey(KeyCode.LeftControl) && UIManager.UIM.gameStart && gameOver == false && (PawnPromoteManager.isPromoting == false || (PawnPromoteManager.isPromoting && PawnPromoteManager.startOver == false))) {//悔棋
                bool isPromoteShutDown = false;
                if (PawnPromoteManager.startOver == false && PawnPromoteManager.isPromoting) {
                    PawnPromoteManager.PPM.SendMessage("PromoteShutDown");
                    isPromoteShutDown = true;
                }
                if (AIMode == true) {
                    if (isPromoteShutDown == true) {
                        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("PopChessMoveInfo");//如果 正在Promote，退一次
                    } else {
                        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("PopChessMoveInfo");//退两次
                        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("PopChessMoveInfo");
                    }
                } else {
                    ChessPieceMoveManager.chessPieceMoveManager.SendMessage("PopChessMoveInfo");//不是AI，不管怎样都退一次
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (UIManager.UIM.chessPieceDisplay) {
                chessboardVisible ^= true;
            }
        }
    }
    private void ChangeTurn(bool next) {
        isWhiteTurn ^= true;
        if (next) {
            NextRound();
        } else {
            ReturnRound();
        }
    }
    private void NextRound() {//如果换过回合后，是先手的回合，那么回合数加一
        if (isWhiteTurn == true) {
            ++round;
        }
    }
    private void ReturnRound() {
        if (isWhiteTurn == true) {
            --round;
        }
    }
    private void SetAIDifficulty() {
        switch (AIDifficulty) {
            case 1:
                AIMaxDepth = 1;
                break;
            case 2:
                AIMaxDepth = 2;
                break;
            case 3:
                AIMaxDepth = 4;
                break;
        }
        int count = 0;
        foreach (GameObject element in ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[0]) {
            ++count;
        }
        foreach (GameObject element in ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[1]) {
            ++count;
        }
    }
    private void WhosWinner(bool isWhite) {
        if (isWhite) {
            print("Winner White!");
        } else {
            print("Winner Black!");
        }
        if (AudioManager.CheckmateSound) {
            AudioManager.AM.SendMessage("PlayCheckmate");//赢了 应该庆祝一下
        }
        gameOver = true;
        UIManager.UIM.SendMessage("WhosWinner", isWhite);
        UIManager.UIM.SendMessage("SwitchGameState", false);
    }
    private void OnCheck(bool enemyIsWhite) { // 谁被Check了
        if (enemyIsWhite) {
            print("White's King in danger!");
        } else {
            print("Black's King in danger!");
        }
        if (CheckUI) {
            UIManager.UIM.SendMessage("OnCheck", enemyIsWhite);
        }
    }
    private void OnCheckmate(bool enemyIsWhite) { // 谁被Checkmate了
        WhosWinner(!enemyIsWhite);
        UIManager.UIM.SendMessage("OnCheckmate", enemyIsWhite);
    }
    private void OnDraw(bool isWhite) { //谁提出的Draw
        if (isWhite)
            print("白方提出严正交涉");
        else
            print("黑方提出严正交涉");
        gameOver = true;
        UIManager.UIM.SendMessage("OnDraw");
        UIManager.UIM.SendMessage("SwitchGameState", false);
    }
    private void RestratAllTheGame() {
        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ShutDown");//游戏移动ShutDown
        Camera.main.SendMessage("ResetSphericalVector");//摄像机控制初始化
        while (ChessPieceMoveManager.chessPieceMoveManager.PopChessMoveInfo() != false) ;//子回归
        foreach (GameObject element in ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[0]) {//黑白双方
            element.transform.position = Vector3.zero;//子放置
            MeshRenderer mr = element.GetComponent<MeshRenderer>();//子显示
            mr.enabled = true;
            element.SendMessage("SetTargetPosition", ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary[element.GetComponent<ChessPieceInfo>().Position].transform.position);//子归位
        }
        foreach (GameObject element in ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[1]) {
            element.transform.position = Vector3.zero;
            MeshRenderer mr = element.GetComponent<MeshRenderer>();
            mr.enabled = true;
            element.SendMessage("SetTargetPosition", ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary[element.GetComponent<ChessPieceInfo>().Position].transform.position);
        }
        gameOver = false;
        round = 1;
        AIDifficulty = 2;
        isWhiteTurn = true;
        isWhitePlayer = true;
        SetAIDifficulty();//准备就绪，只限开关打开
    }
    public void ReLoadGame() {
        SceneManager.LoadScene(0);
    }
    public void OnGameExit() {
        Application.Quit();
    }
}
