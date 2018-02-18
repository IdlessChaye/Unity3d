using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//右键有三种用法：0停止跟随 1普通右键 切换 2高亮右键 切换 4左右键一起前进 5右键停止旋转 
public class CameraFollow : MonoBehaviour {
    public float mouseSpeed;
    public float smoothingCamera;
    public float smoothingTarget;
    public Transform targetTf;
    public float changeInterval;
    public float advanceInterval;
    public float advancingSpeed;
    public GameObject emptyAdvancing;
    public MouseMode mouseMode = MouseMode.Normal;
    public static SphericalVector sphericalVector = new SphericalVector(50f, 0.5f, 1f);

    private float h, v, m;
    private Ray ray;
    private RaycastHit[] hits;
    private int layerMask;
    private Transform currentLookAtTf;
    private bool rollingView;
    private float lastOnChangeTime;
    private bool keepHighlight;
    private bool startCountingAdvance;
    private float lastOnAdvanceTime;
    private GameObject advancingGameObject;
    private GameObject centerGameObject;

    public static bool CullingMeshFadeFX { get; set; }
    private void SetBools() {
        CullingMeshFadeFX = false;
    }

    private void Awake() {
        layerMask = LayerMask.GetMask("Background", "HighlightBackground", "SelectedBackground", "StartBackground");
        currentLookAtTf = new GameObject("CurrentLookAtGameObject").transform;
        currentLookAtTf.position = targetTf.position;
        rollingView = true;
        keepHighlight = false;
        startCountingAdvance = false;
        centerGameObject = new GameObject("Center");
        centerGameObject.transform.position = Vector3.zero;
        SetBools();
    }
    private void Start() {
        ChangeTarget(targetTf.gameObject);
        ChangeToMainManuPosition();
    }
    // private bool changingView;
    void Update() {
        if (UIManager.UIM.gameStart && PawnPromoteManager.isPromoting == false) {
            if (Input.GetButtonDown("Fire3")) {
                if (mouseMode == MouseMode.Follow)
                    mouseMode = MouseMode.Normal;
                else
                    mouseMode = MouseMode.Follow;
            }
            PushingView();
            if (rollingView) {
                h = Input.GetAxis("Mouse X");
                v = Input.GetAxis("Mouse Y");
                h = Mathf.Abs(h) < 0.31f ? 0f : h;
                v = Mathf.Abs(v) < 0.31f ? 0f : v;
                sphericalVector.azimuth += h * mouseSpeed;
                sphericalVector.zenith -= v * mouseSpeed;
                sphericalVector.zenith = Mathf.Clamp(sphericalVector.zenith, -1f, 1f);
            }
            switch (mouseMode) {
                case MouseMode.Follow://Follow模式，左右键都有功能
                    transform.position = Vector3.Lerp(transform.position, targetTf.position + sphericalVector.Position, smoothingCamera * Time.deltaTime);
                    if (advancingGameObject == null) {
                        currentLookAtTf.position = Vector3.Lerp(currentLookAtTf.position, targetTf.position, smoothingTarget * Time.deltaTime);
                        transform.LookAt(currentLookAtTf);
                    } else {
                        transform.LookAt(advancingGameObject.transform);
                    }
                    if (CullingMeshFadeFX) {
                        CullingMesh();
                    }
                    if (Input.GetButtonDown("Fire2")) {
                        lastOnChangeTime = Time.time;
                    }
                    if (Input.GetButtonUp("Fire2")) {
                        if (Time.time - lastOnChangeTime <= changeInterval) {
                            RandomChangeTarget();
                        }
                        if (advancingGameObject != null) {
                            ChessPieceMoveManager.chessPieceMoveManager.enabled = true;
                            DestroyAdvancing();
                        }
                        startCountingAdvance = false;
                    }
                    if (Input.GetButtonDown("Fire1")) {
                    }
                    if (Input.GetButtonUp("Fire1")) {
                        if (advancingGameObject != null) {
                            ChessPieceMoveManager.chessPieceMoveManager.enabled = true;
                            DestroyAdvancing();
                        }
                        keepHighlight = false;
                        startCountingAdvance = false;
                    }
                    if (Input.GetButton("Fire2") && advancingGameObject == null) {
                        OnRollingView(false);
                    } else {
                        OnRollingView(true);
                    }
                    break;
                case MouseMode.Normal://Normal模式，只有左键有功能
                    if (Vector3.Distance(currentLookAtTf.transform.position, centerGameObject.transform.position) > 0.1f) {
                        OnRollingView(false);
                        transform.position = Vector3.Lerp(transform.position, centerGameObject.transform.position + sphericalVector.Position, smoothingTarget * Time.deltaTime);
                    } else {
                        transform.position = Vector3.zero + sphericalVector.Position;
                        if (Input.GetButton("Fire1")) {
                            OnRollingView(true);
                        } else {
                            OnRollingView(false);
                        }
                        if (Input.GetButtonDown("Fire2")) {
                            ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ShutDown");
                        }
                    }
                    currentLookAtTf.position = Vector3.Lerp(currentLookAtTf.position, centerGameObject.transform.position, smoothingTarget * Time.deltaTime);
                    transform.LookAt(currentLookAtTf);
                    break;
            }
        }
    }
    private void FixedUpdate() {
        if (UIManager.UIM.gameStart && PawnPromoteManager.isPromoting == false) {
            switch (mouseMode) {
                case MouseMode.Follow:
                    if (startCountingAdvance == false) {
                        if (Input.GetButton("Fire1") && Input.GetButton("Fire2")) {
                            startCountingAdvance = true;
                        }
                        lastOnAdvanceTime = Time.time;
                    }
                    if (Input.GetButton("Fire2") && Input.GetButton("Fire1") && Time.time - lastOnAdvanceTime >= advanceInterval) {//如何在这里keepHighlight呢？迷
                        if (advancingGameObject == null) {
                            ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ShutDown");
                            ChessPieceMoveManager.chessPieceMoveManager.enabled = false;
                            advancingGameObject = Instantiate(emptyAdvancing, targetTf.position, Quaternion.identity);
                            targetTf = advancingGameObject.transform;
                        }
                        advancingGameObject.transform.Translate(Quaternion.AngleAxis(-23f, transform.right) * transform.forward * advancingSpeed);
                    }
                    break;
            }
        }
    }
    private void PushingView() {
        m = Input.GetAxis("Mouse ScrollWheel");
        sphericalVector.length -= 20f * m;
        sphericalVector.length = Mathf.Clamp(sphericalVector.length, 2f, Mathf.Infinity);
    }
    private void DestroyAdvancing() {
        GameObject target;
        float distance;
        int isWhite = 1;
        if (ChessManager.CM.isWhiteTurn)
            isWhite = 0;
        target = ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[isWhite][0];
        distance = Vector3.Distance(advancingGameObject.transform.position, target.transform.position);
        foreach (GameObject element in ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[isWhite]) {
            float d = Vector3.Distance(advancingGameObject.transform.position, element.transform.position);
            if (d < distance) {
                target = element;
                distance = d;
            }
        }
        targetTf = target.transform;
        currentLookAtTf.position = advancingGameObject.transform.position;
        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ShutDown");
        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ThisChangeTarget", target);
        ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ShutDown");
        Destroy(advancingGameObject);
        advancingGameObject = null;
    }
    private void ChangeTarget(GameObject target) {
        if (targetTf != null && advancingGameObject == null && target.layer == LayerMask.NameToLayer("Player"))
            if (targetTf.GetComponent<ChessPieceMove>() != null)
                targetTf.GetComponent<ChessPieceMove>().IsLocked = false;
        targetTf = target.transform;
    }
    private void RandomChangeTarget() {//随机到己方其他棋子
        int indexIsWhite = 1;
        if (ChessManager.CM.isWhiteTurn)
            indexIsWhite = 0;
        GameObject go;
        do {
            go = ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[indexIsWhite][Random.Range(0, ChessPieceMoveManager.chessPieceMoveManager.chessPiecesLists[indexIsWhite].Count)];
        } while (targetTf == go.transform);
        ChangeTarget(go);
        if (ChessPieceMoveManager.chessPieceMoveManager.hasHighlightMesh || keepHighlight) {
            ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ThisChangeTarget", go);
            keepHighlight = true;
        }
    }
    private void CullingMesh() {
        ray.origin = transform.position; //剔除遮挡mesh
        ray.direction = targetTf.position - transform.position;
        hits = Physics.RaycastAll(ray, Vector3.Distance(targetTf.position, transform.position), layerMask);
        foreach (RaycastHit hit in hits) {
            hit.collider.SendMessage("Disappear");
        }
    }
    private void OnRollingView(bool rolling) {
        rollingView = rolling;
    }
    private void ChangeToGamePosition() {
        StopAllCoroutines();
        StartCoroutine(LerpToGamePosition());
    }
    IEnumerator LerpToGamePosition() {
        float t = 0;
        float timeInterval = 1f;
        GameObject ob = GameObject.Find("GamestartPosition");
        Vector3 position = ob.transform.position;
        Quaternion rotation = ob.transform.rotation;
        while (t < timeInterval) {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, t / timeInterval);
            transform.position = Vector3.Lerp(transform.position, position, t / timeInterval);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return 0;
    }
    private void ChangeToMainManuPosition() {
        StopAllCoroutines();
        StartCoroutine(LerpToMainManuPosition());
    }
    IEnumerator LerpToMainManuPosition() {
        float t = 0;
        float timeInterval = 1f;
        GameObject ob = GameObject.Find("UIStartPosition");
        Vector3 position = ob.transform.position;
        Quaternion rotation = ob.transform.rotation;
        while (t < timeInterval) {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, t / timeInterval);
            transform.position = Vector3.Lerp(transform.position, position, t / timeInterval);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return 0;
    }
    void ResetSphericalVector() {
        sphericalVector.length = 50f;
        sphericalVector.zenith = 0.5f;
        sphericalVector.azimuth = 1f;
    }
}