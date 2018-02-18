using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnPromoteManager : MonoBehaviour {
    public static PawnPromoteManager pawnPromoteManager;
    public static PawnPromoteManager PPM {
        get {
            return pawnPromoteManager;
        }
    }

    public static bool isPromoting;
    public GameObject[] promotions;

    private GameObject pawn;
    private bool isWhite;
    private Vector3 startPosition;
    private GameObject[] waters;
    private bool shootWater;//是否可以射线
    private Vector3 cameraStartPosition;
    private Quaternion cameraStartRotation;
    public static bool startOver;

    private void Awake() {
        pawnPromoteManager = this;
        isPromoting = false;
        layerWater = LayerMask.GetMask("Water");
        shootWater = false;
        waters = new GameObject[4];
        startOver = true;
    }
    private Ray ray;
    private RaycastHit hit;
    private int layerWater;
    private void Update() {
        if (shootWater && Input.GetButtonDown("Fire1")) {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerWater)) {
                startOver = true;
                StartCoroutine(LerpToPosition(hit.collider.gameObject, startPosition, pawn.transform.rotation, 3));
                foreach (GameObject element in waters) {
                    if (element != hit.collider.gameObject) {
                        Destroy(element);
                    }
                }
                AudioManager.AM.SendMessage("PlayPromote");
                shootWater = false;
            }
        }
    }
    private void StartPromotion(GameObject pawn) {
        isPromoting = true;
        startOver = false;
        shootWater = false;
        this.pawn = pawn;
        startPosition = ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary[pawn.GetComponent<ChessPieceInfo>().Position].transform.position;//得到pawn的世界坐标
        float zDirection, xDirection;
        isWhite = pawn.GetComponent<ChessPieceInfo>().isWhite;
        Material material;
        if (isWhite) {
            zDirection = 1f;
            xDirection = -1f;
            material = Resources.Load("Materials/PlayerWhite") as Material;
        } else {
            zDirection = -1f;
            xDirection = 1f;
            material = Resources.Load("Materials/PlayerBlack") as Material;
        }
        float positionSpace = ChessboardManager.Length / 3;//替换子间的间隔
        Vector3 targetPosition = startPosition + Vector3.forward * zDirection * ChessboardManager.Length + Vector3.right * xDirection * positionSpace * 3 / 2;//依次生成替换子的位置
        cameraStartPosition = Camera.main.transform.position;//设置Camera动作
        cameraStartRotation = Camera.main.transform.rotation;
        GameObject lookGO = new GameObject("LookGameObject");
        Destroy(lookGO, 2f);
        lookGO.transform.position = startPosition + Vector3.up * 13f + Vector3.forward * zDirection * -1 * 8;
        lookGO.transform.rotation = Quaternion.LookRotation(startPosition - lookGO.transform.position + Vector3.forward * zDirection * positionSpace);
        StartCoroutine(LerpToPosition(Camera.main.gameObject, lookGO.transform.position, lookGO.transform.rotation, 1));
        for (int i = 0; i < 4; ++i) {
            GameObject go = Instantiate(promotions[i], startPosition, Quaternion.LookRotation(Vector3.forward * zDirection * -1));
            waters[i] = go;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            StartCoroutine(LerpToPosition(go, targetPosition, Quaternion.identity));
            targetPosition += Vector3.right * xDirection * -1 * positionSpace;
        }
    }
    IEnumerator LerpToPosition(GameObject go, Vector3 targetPosition, Quaternion targetRotation, int type = 0) {
        float t = 0f;
        float timeInterval = 0.8f;
        while (t < timeInterval) {
            t += Time.deltaTime;
            if (type == 0) {
                go.transform.position = Vector3.Lerp(go.transform.position, targetPosition, t / timeInterval);
            } else if (type == 1) {
                go.transform.position = Vector3.Slerp(go.transform.position, targetPosition, t / timeInterval);
                go.transform.rotation = Quaternion.Lerp(go.transform.rotation, targetRotation, t / timeInterval);
            } else if (type == 2 || type == 3) {
                go.transform.position = Vector3.Lerp(go.transform.position, targetPosition, t / timeInterval);
                go.transform.rotation = Quaternion.Lerp(go.transform.rotation, targetRotation, t / timeInterval);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        if (type == 3) {//清理Promotions
            for (int i = 0; i < waters.Length; ++i) {
                if (waters[i] == go) {
                    EndPromotion(go);
                    Destroy(go);
                }
            }
        } else if (type == 2) {//Camera回归就位后
            isPromoting = false;
        } else if (type == 0) {
            shootWater = true;//反正type0只有棋子散开的情况,可以开启射线
        }
        yield return 0;
    }
    private void EndPromotion(GameObject become) {//promotion收尾工作
        //模型复制标签网格Collider 销毁 PushPop camera归位
        ChangeAppearance(pawn, become, become.tag);
        ChessPromoteInfo chessPromoteInfo = new ChessPromoteInfo(isWhite, pawn, startPosition, startPosition, "Pawn", become.tag);
        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("PushChessMoveInfo", chessPromoteInfo);
        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("AfterMoveCheck", !isWhite);
        StartCoroutine(LerpToPosition(Camera.main.gameObject, cameraStartPosition, cameraStartRotation, 2));
        if (ChessManager.AIMode == true) {
            Invoke("ExecuteAIShowTime", 0.8f);//玩家操作完再切换到AIPlay ，0.8秒是摄像机移动的时间
        }
    }
    private void ExecuteAIShowTime() {
        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("AIShowTime", ChessManager.AIDifficulty);
    }
    public static void ChangeAppearance(GameObject who, GameObject become, string tag) {
        who.tag = tag;//获取标签
        Mesh mesh = new Mesh();//还有网格
        Vector3 scale = Vector3.zero;
        switch (tag) {
            case "Pawn":
                scale = new Vector3(0.4f, 0.385f, 0.4f);
                break;
            case "Bishop":
                scale = new Vector3(0.4f, 0.39f, 0.4f);
                break;
            case "Knight":
                scale = new Vector3(0.4f, 0.41f, 0.4f);
                break;
            case "Queen":
                scale = new Vector3(0.42f, 0.42f, 0.42f);
                break;
            case "Rook":
                scale = new Vector3(0.4f, 0.44f, 0.4f);
                break;
        }
        who.transform.localScale = scale;//还有大小，其他的不用从become那里拿了
        mesh.vertices = become.GetComponent<MeshFilter>().mesh.vertices;
        mesh.triangles = become.GetComponent<MeshFilter>().mesh.triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        who.GetComponent<MeshFilter>().sharedMesh = mesh;
        who.GetComponent<MeshCollider>().sharedMesh = mesh;
        bool isWhite = who.GetComponent<ChessPieceInfo>().isWhite;
        if (isWhite) {
            who.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/PlayerWhite") as Material;
        } else {
            who.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Materials/PlayerBlack") as Material;
        }
    }
    private void PromoteShutDown() {//只有还未promote时可以悔棋，选择升格->回位没有升格 摄像机移动
        StopAllCoroutines();
        for (int i = 0; i < waters.Length; ++i) {
            if (waters[i] != null)
                Destroy(waters[i]);
        }
        shootWater = false;
        pawn = null;
        startPosition = Vector3.zero;
        isPromoting = true;
        StartCoroutine(LerpToPosition(Camera.main.gameObject, cameraStartPosition, cameraStartRotation, 2));//相机信息要留着
    }
    private void ShutDownToMainManu() { //为了在即将promote前退出游戏的函数 摄像机停止，什么都清空
        isPromoting = false;
        StopAllCoroutines();
        for (int i = 0; i < waters.Length; ++i) {
            if (waters[i] != null)
                Destroy(waters[i]);
        }
        shootWater = false;
        pawn = null;
        startPosition = Vector3.zero;
        cameraStartPosition = Vector3.zero;
        cameraStartRotation = Quaternion.identity;
        startOver = true;
    }
}
