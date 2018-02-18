using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager uiManager;
    public static UIManager UIM {
        get {
            return uiManager;
        }
    }
    public bool gameStart;//放这就放这吧。。
    public bool chessPieceDisplay;
    public bool MapHolderRotate;
    public UICamera uiCamera;
    public CursorLockMode wantedMode;

    private GameObject panelMain;
    private GameObject panelGameChoose;
    private GameObject panelGame;
    private GameObject panelOption;
    private GameObject panelESC;
    public static GameObject panelReturn;//NGUI与UGUI都需要知道的变量
    private GameObject[] panelCurrentPast = new GameObject[2];

    private void Awake() {
        uiManager = this;
        gameStart = false;
        chessPieceDisplay = false;
        MapHolderRotate = false;
        Cursor.lockState = wantedMode;
    }
    private void Start() {
        panelMain = GameObject.Find("Panel - Main");
        panelGameChoose = GameObject.Find("Panel - GameChoose");
        panelGame = GameObject.Find("Panel - Game");
        panelOption = GameObject.Find("Panel - Option");
        panelESC = GameObject.Find("Panel - ESC");
        labelAddPetrol = GameObject.Find("Label - AddPetrol");
        labelUnity = GameObject.Find("Label - Unity");
        labelCheck = GameObject.Find("Label - Check");
        labelCheckmate = GameObject.Find("Label - Checkmate");
        labelWinner = GameObject.Find("Label - Winner");
        labelDraw = GameObject.Find("Label - Draw");
        labelIdless = GameObject.Find("Label - Idless");
        labelPromote = GameObject.Find("Label - Promote");
        InitializeUI();
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            //判断 如果是MainManu ESC 如果是GameChoose MainManu 如果是Game ESC 如果是ESC 返回上次panel 如果是Option 返回上次panel
            switch (panelCurrentPast[0].name) {
                case "Panel - Main":
                    if (UGUIManager.uguiIsWorking) {//如果退出游戏界面
                        UGUIManager.UGUIM.SendMessage("OffUGUI");
                        break;
                    }
                    SwitchToPanelESC();//如果只是主界面
                    break;
                case "Panel - GameChoose":
                    SwitchToPanelMain();
                    break;
                case "Panel - Game":
                    SwitchToPanelESC();
                    break;
                case "Panel - Option":
                    switch (panelCurrentPast[1].name) {
                        case "Panel - ESC":
                            SwitchToPanelESC();
                            break;
                        case "Panel - Main":
                            SwitchToPanelMain();
                            break;
                    }
                    break;
                case "Panel - ESC":
                    if (UGUIManager.uguiIsWorking) {
                        UGUIManager.UGUIM.SendMessage("OffUGUI");
                        break;
                    }
                    switch (panelReturn.name) {
                        case "Panel - Main":
                            SwitchToPanelMain();
                            break;
                        case "Panel - Game":
                            SwitchToPanelGame();
                            break;
                    }
                    break;
            }
        }
    }
    private void SwitchGameState(bool gameStart) {//不能操控鼠标棋子，可以操控鼠标棋子
        this.gameStart = gameStart;
        CheckCursorMode();
    }
    private void CheckCursorMode() {//游戏暂停鼠标就可以随便动了
        if (gameStart) {
            Cursor.lockState = CursorLockMode.Confined;//游戏开始的时候鼠标不能出去
        } else {
            Cursor.lockState = CursorLockMode.None;
        }
    }
    private void SwitchPanel(GameObject panel) {
        ShutDownPanels();
        panel.SetActive(true);
        switch (panel.name) {
            case "Panel - Main":
                panelReturn = panelMain;
                break;
            case "Panel - Game":
                panelReturn = panelGame;
                break;
        }
        panelCurrentPast[1] = panelCurrentPast[0];
        panelCurrentPast[0] = panel;
    }
    private void ShutDownPanels() {
        panelMain.SetActive(false);
        panelGameChoose.SetActive(false);
        panelGame.SetActive(false);
        panelOption.SetActive(false);
        panelESC.SetActive(false);
    }
    private GameObject labelWinner;
    private void WhosWinner(bool isWhite) {
        SwitchPanel(panelGame);
        labelWinner.GetComponent<UILabel>().text = "Winner " + (isWhite ? "White" : "Black");
        labelWinner.SetActive(true);
    }
    private GameObject labelCheck;
    private void OnCheck(bool enemyIsWhite) { // 谁被Check了
        SwitchPanel(panelGame);
        labelCheck.SetActive(true);
    }
    private GameObject labelCheckmate;
    private void OnCheckmate(bool enemyIsWhite) { // 谁被Checkmate了
        SwitchPanel(panelGame);
        labelCheckmate.SetActive(true);
    }
    private void OnDraw() { //谁提出的Draw
        LabelDraw();
    }
    private void InitializeUI() {
        labelCheck.SetActive(false);
        labelCheckmate.SetActive(false);
        labelWinner.SetActive(false);
        labelAddPetrol.SetActive(false);
        labelUnity.SetActive(false);
        labelDraw.SetActive(false);
        labelIdless.SetActive(false);
        labelPromote.SetActive(false);
        panelCurrentPast[0] = panelMain;
        SwitchPanel(panelMain);
    }
    public void SwitchToPanelMain() {
        if (panelCurrentPast[0] == panelESC && panelReturn == panelGame) {//从游戏退出才设置
            PawnPromoteManager.PPM.SendMessage("ShutDownToMainManu");//解决从升格途中退出游戏的BUG
            foreach (GameObject element in DeadListManager.DLM.deadLists[0].GetComponent<DeadList>().deads) { //dead棋子renderer false
                element.GetComponent<MeshRenderer>().enabled = false;
            }
            foreach (GameObject element in DeadListManager.DLM.deadLists[1].GetComponent<DeadList>().deads) {
                element.GetComponent<MeshRenderer>().enabled = false;
            }
            foreach (GameObject element in ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[0]) { //棋子renderer false
                element.GetComponent<MeshRenderer>().enabled = false;
            }
            foreach (GameObject element in ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[1]) {
                element.GetComponent<MeshRenderer>().enabled = false;
            }
            foreach (GameObject element in AudioManager.AM.GetComponent<LetsDropTheBeat>().jumpPieces) {//背景跳棋子工作
                element.SetActive(true);
            }
            ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ShutDown");//棋子移动信息清空
            AudioManager.AM.SendMessage("ResetAudioSource");//音乐清空
            ChessManager.AIMode = false;
            ChessManager.AIZHIZHANG = false;
            chessPieceDisplay = false;  //棋子不动
            SwitchGameState(false); //游戏开关关闭
        }
        Camera.main.SendMessage("ChangeToMainManuPosition");//主相机移位 GameChoose -> Main
        SwitchPanel(panelMain);
    }
    public void SwitchToPanelGameChoose() {
        SwitchPanel(panelGameChoose);
        Camera.main.SendMessage("ChangeToGamePosition");//主相机移位
    }
    public void SwitchToPanelGame() {
        SwitchPanel(panelGame);
        SwitchGameState(true);
    }
    public void SwitchToPanelOption() {
        SwitchPanel(panelOption);
        UIOptionManager.UIOM.SendMessage("StartOption");
        SwitchGameState(false);
    }
    public void SwitchToPanelESC() {
        SwitchPanel(panelESC);
        SwitchGameState(false);
    }
    public void OnVegetables() {
        ChessManager.AIMode = true;
        ChessManager.AIZHIZHANG = true;
        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ResetPlayerIsWh",true);
        SwitchPanel(panelGame);
        MeshRendererManager.SetAudioEffectFactor();
        AudioManager.AM.SendMessage("WeakAudioVolume"); //音乐ReSet
        MapHolderRotate = false;//棋盘停止旋转
        StartCoroutine(ReSetMapHolder());//mapHolder归位
        foreach (GameObject element in AudioManager.AM.GetComponent<LetsDropTheBeat>().jumpPieces) {//背景跳棋子停机
            element.SetActive(false);
        }
        Invoke("OnPlayerDelay", 1f);
    }
    public void OnRobot() {
        ChessManager.AIMode = true;
        ChessManager.AIZHIZHANG = false;
        SwitchPanel(panelGame);
        MeshRendererManager.SetAudioEffectFactor();
        AudioManager.AM.SendMessage("WeakAudioVolume"); //音乐ReSet
        MapHolderRotate = false;//棋盘停止旋转
        StartCoroutine(ReSetMapHolder());//mapHolder归位
        foreach (GameObject element in AudioManager.AM.GetComponent<LetsDropTheBeat>().jumpPieces) {//背景跳棋子停机
            element.SetActive(false);
        }
        Invoke("OnPlayerDelay", 1f);
    }
    public void OnPlayer() {// GameChoose -> Game 关闭MainManu（UI,音乐，MapHolder,JumpPieces)，开启GameUI
        ChessManager.AIMode = false;
        ChessManager.AIZHIZHANG = false;
        SwitchPanel(panelGame);
        MeshRendererManager.SetAudioEffectFactor();
        AudioManager.AM.SendMessage("WeakAudioVolume"); //音乐ReSet
        MapHolderRotate = false;//棋盘停止旋转
        StartCoroutine(ReSetMapHolder());//mapHolder归位
        foreach (GameObject element in AudioManager.AM.GetComponent<LetsDropTheBeat>().jumpPieces) {//背景跳棋子停机
            element.SetActive(false);
        }
        Invoke("OnPlayerDelay", 1f);
    }
    public void OnNet() {
        LabelDelay();
    }
    public void OnEscStaff() {
        LabelUnity();
        LabelIdless();
    }
    public void OnCool() {
        LabelDelay();
    }
    IEnumerator ReSetMapHolder() {
        float t = 0f;
        float timeInterval = 1f;
        Transform mhTF = ChessboardManager.mapHolder;
        //while (mhTF.rotation != Quaternion.identity || mhTF.position != Vector3.zero) {
        while (t < timeInterval) {
            t += Time.deltaTime;
            mhTF.rotation = Quaternion.Lerp(mhTF.rotation, Quaternion.identity, t / timeInterval);
            mhTF.position = Vector3.Lerp(mhTF.position, Vector3.zero, t / timeInterval);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return 0;
    }
    private void OnPlayerDelay() {
        ChessManager.CM.SendMessage("RestratAllTheGame");//摄像机，棋子初始化
        chessPieceDisplay = true;  //棋子开动
        SwitchGameState(true);//游戏开关开启
    }

    private void NoEvent() {
        uiCamera.eventReceiverMask = 0;
    }
    private void UIEvent() {
        uiCamera.eventReceiverMask = LayerMask.GetMask("UI");
    }

    private GameObject labelAddPetrol;
    private void LabelDelay() {
        labelAddPetrol.SetActive(true);
    }
    private GameObject labelUnity;
    private void LabelUnity() {
        labelUnity.SetActive(true);
    }
    private GameObject labelDraw;
    private void LabelDraw() {
        labelDraw.SetActive(true);
    }
    private GameObject labelIdless;
    private void LabelIdless() {
        labelIdless.SetActive(true);
    }
    private GameObject labelPromote;
    private void LabelPromote() {
        labelPromote.SetActive(true);
    }
}
