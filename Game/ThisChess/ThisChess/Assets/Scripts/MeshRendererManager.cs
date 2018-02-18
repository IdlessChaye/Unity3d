using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRendererManager : MonoBehaviour {
    public static Color[] colorTransitionArray = new Color[7] {
        new Color(1,0,0),new Color(1,0,1),new Color(0,0,1),new Color(0,1,1),new Color(0,1,0),new Color(1,1,0),new Color(1,0,0)
    };
    [SerializeField] private Vector3 position;
    public static float waveSendInterval = 0.09f; //0.09
    public static float waveChangeInterval = 0.13f; //0.13
    public static float smoothing = 4f; // 4

    public static void SetMusicFactor() {
        waveSendInterval = 0.05f;
        waveChangeInterval = 0.07f;
        smoothing = 4f;
    }
    public static void SetAudioEffectFactor() {
        waveSendInterval = 0.09f;
        waveChangeInterval = 0.13f;
        smoothing = 4f;
    }
    public Vector3 Position {
        get {
            return position;
        }
        set {
            position = value;
        }
    }
    private float lastCallTime;
    private float timeInterval;
    private bool isDisappear;
    private bool isWaving;
    private Material normalMaterial;
    private Material highlightMaterial;
    private Material attackMaterial;
    private Material selectedMaterial;
    private Material startMaterial;
    private Material transitionMaterial;
    private MeshRenderer meshRenderer;
    private float level;
    private float alphaValue;
    private int colorIndex;
    private Color targetColor;
    private float alpha = 30f;
    public static bool StartMeshFX { get; set; }
    public static bool HighlightMeshFX { get; set; }
    public static bool AttackMeshFX { get; set; }
    public static bool SelectedMeshFX { get; set; }

    private void SetBools() {
        StartMeshFX = true;
        HighlightMeshFX = true;
        AttackMeshFX = true;
        SelectedMeshFX = true;
    }

    void Awake() {
        timeInterval = 1f;
        isDisappear = false;
        highlightMaterial = Resources.Load("Materials/BackGroundHighlight") as Material;
        attackMaterial = Resources.Load("Materials/BackGroundAttack") as Material;
        selectedMaterial = Resources.Load("Materials/BackGroundSelected") as Material;
        startMaterial = Resources.Load("Materials/BackGroundStart") as Material;
        transitionMaterial = Resources.Load("Materials/Transition") as Material;
        alphaValue = transitionMaterial.color.a;
        for (int i = 0; i < colorTransitionArray.Length; ++i) {
            colorTransitionArray[i].a = alphaValue;
        }
        meshRenderer = GetComponent<MeshRenderer>();
        isWaving = false;
        level = ChessboardManager.Level;
        SetBools();
    }
    private void Start() {
        if ((int)(Position.x + Position.y) % 2 == 0) {
            normalMaterial = Resources.Load("Materials/BackGroundNormalWhite") as Material;
        } else {
            normalMaterial = Resources.Load("Materials/BackGroundNormalBlack") as Material;
        }
        OnNormalMaterial();
    }
    private void FixedUpdate() {
        if (meshRenderer.material.color != targetColor)
            meshRenderer.material.color = Color.Lerp(meshRenderer.material.color, targetColor, smoothing * Time.deltaTime);
    }
    private Color myTargetColor;
    private string stepTag;
    private void StartWave(string stepTag) {
        if (isWaving) {
            if (!(stepTag == "Boom" && this.stepTag != "Boom")) {
                return;
            }
        }
        this.stepTag = stepTag;
        colorIndex = 0;
        myTargetColor = targetColor;
        targetColor = colorTransitionArray[0];
        lastWaveTime = Time.time;
        isWaving = true;
        StartCoroutine(Waving());
        Invoke("SendWaveMassage", waveSendInterval);
    }
    private float lastWaveTime;
    IEnumerator Waving() {
        while (true) {
            if (Time.time - lastWaveTime >= waveChangeInterval) {
                colorIndex++;
                if (colorIndex >= colorTransitionArray.Length) {
                    if (stepTag == "Boom") {
                        targetColor = normalMaterial.color;
                    } else {
                        targetColor = myTargetColor;
                    }
                    isWaving = false;
                    break;
                }
                targetColor = colorTransitionArray[colorIndex];
                lastWaveTime = Time.time;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return 0;
    }
    private void SendWaveMassage() {
        if (GetNextWaveMeshPositionList(stepTag)) {
            foreach (Vector3 element in nextMeshPositionLinkedList) {
                GameObject nextMesh = ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary[element];
                nextMesh.SendMessage("StartWave", stepTag);
            }
        }
    }
    private LinkedList<Vector3> nextMeshPositionLinkedList = new LinkedList<Vector3>();
    private Vector3 nextPosition;
    private bool GetNextWaveMeshPositionList(string stepTag) {
        nextMeshPositionLinkedList.Clear();
        switch (stepTag) {
            case "Boom":
            case "King":
                Boom();
                break;
            case "Pawn":
                Pawn();
                break;
            case "Rook":
                Rook();
                break;
            case "Bishop":
                Bishop();
                break;
            case "Queen":
                Queen();
                break;
            case "Knight":
                Knight();
                break;
        }
        TestNextMeshPositionLinkedList();
        return nextMeshPositionLinkedList.Count != 0;
    }
    private void Boom() {
        nextPosition = position;
        nextPosition.z = position.z + 1;
        if (position.z >= 0) { //斜上方
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x -= 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y -= 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x += 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
        } else {
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x += 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y += 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x -= 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
        }
        nextPosition = position;
        nextPosition.z = position.z - 1;
        if (position.z > 0) { //斜下方
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y += 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x += 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y -= 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
        } else {
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y -= 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x -= 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y += 1;
            nextMeshPositionLinkedList.AddFirst(nextPosition);
        }
        nextPosition = position;//→←↑↓
        nextPosition.x += 1;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x -= 2;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x += 1;
        nextPosition.y += 1;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.y -= 2;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
    }
    private void Pawn() {
        nextPosition = position;
        if (ChessManager.CM.isWhiteTurn) //白方回合
            nextPosition.y += 1;
        else //黑方回合
            nextPosition.y -= 1;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x = position.x - 1;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x = position.x + 1;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition = position;
        nextPosition.z += 1;
        if (ChessManager.CM.isWhiteTurn) {
            if (nextPosition.z > 0) {//斜前上方
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
            } else {
                nextPosition.y += 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
            }
        } else {
            if (nextPosition.z > 0) {//斜前上方
                nextPosition.y -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
            } else {
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
            }
        }
        nextPosition = position;
        nextPosition.z -= 1;
        if (ChessManager.CM.isWhiteTurn) {
            if (nextPosition.z < 0) {
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
            } else {
                nextPosition.y += 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
            }
        } else {
            if (nextPosition.z < 0) {
                nextPosition.y -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
            } else {
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
            }
        }
    }
    private void Rook() {
        float maxIndex = level - Mathf.Abs(position.z);
        nextPosition = position;
        nextPosition.y += 1;
        while (nextPosition.y <= maxIndex) { //↑
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y += 1;
        }
        nextPosition = position;
        nextPosition.y -= 1;
        while (nextPosition.y > 0) { //↓
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y -= 1;
        }
        nextPosition = position;
        nextPosition.x += 1;
        while (nextPosition.x <= maxIndex) { // →
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x += 1;
        }
;
        nextPosition = position;
        nextPosition.x -= 1;
        while (nextPosition.x > 0) { // ←
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x -= 1;
        }
    }
    private void Bishop() {
        nextPosition = position;
        int offset = 1;
        nextPosition.z += 1;
        while (nextPosition.z < level) {//向上走
            if (nextPosition.z <= 0) {
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y -= offset;
            } else {
                nextPosition.x -= 1;
                nextPosition.y -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y -= offset;
            }
            ++offset;
            nextPosition.z += 1;
        }
        nextPosition = position;
        offset = 1;

        nextPosition.z -= 1;
        while (nextPosition.z > -level) { // 向下走
            if (nextPosition.z >= 0) {
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y -= offset;
            } else {
                nextPosition.x -= 1;
                nextPosition.y -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y -= offset;
            }
            ++offset;
            nextPosition.z -= 1;
        }
    }
    private void Queen() {

        float maxIndex = level - Mathf.Abs(position.z);
        nextPosition = position;
        nextPosition.y += 1;
        while (nextPosition.y <= maxIndex) { //↑
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y += 1;
        }
        nextPosition = position;
        nextPosition.y -= 1;
        while (nextPosition.y > 0) { //↓
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.y -= 1;
        }
        nextPosition = position;
        nextPosition.x += 1;
        while (nextPosition.x <= maxIndex) { // →
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x += 1;
        }
        nextPosition = position;
        nextPosition.x -= 1;
        while (nextPosition.x > 0) { // ←
            nextMeshPositionLinkedList.AddFirst(nextPosition);
            nextPosition.x -= 1;
        }
        nextPosition = position;
        int offset = 1;
        nextPosition.z += 1;
        while (nextPosition.z < level) {//向上走
            if (nextPosition.z <= 0) {
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y -= offset;
            } else {
                nextPosition.x -= 1;
                nextPosition.y -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y -= offset;
            }
            ++offset;
            nextPosition.z += 1;
        }
        nextPosition = position;
        offset = 1;

        nextPosition.z -= 1;
        while (nextPosition.z > -level) { // 向下走
            if (nextPosition.z >= 0) {
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y -= offset;
            } else {
                nextPosition.x -= 1;
                nextPosition.y -= 1;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y += offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.x -= offset;
                nextMeshPositionLinkedList.AddFirst(nextPosition);
                nextPosition.y -= offset;
            }
            ++offset;
            nextPosition.z -= 1;
        }
    }
    private void Knight() {

        nextPosition = position; // 向上跳
        int num = 1;
        if (nextPosition.z < 0)
            num = -1;
        nextPosition.z += 1;
        nextPosition.x += 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.y -= 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x -= 3 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.y += 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x += 1 * num;
        nextPosition.y += 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x += 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.y -= 3 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x -= 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition = position; // 往下跳
        num = 1;
        if (nextPosition.z <= 0)
            num = -1;
        nextPosition.z -= 1;
        nextPosition.x -= 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.y += 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x += 3 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.y -= 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x -= 1 * num;
        nextPosition.y -= 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x -= 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.y += 3 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
        nextPosition.x += 1 * num;
        nextMeshPositionLinkedList.AddFirst(nextPosition);
    }
    private void TestNextMeshPositionLinkedList() {
        float maxIndex;
        Vector3[] positionArray;
        positionArray = new Vector3[nextMeshPositionLinkedList.Count];
        nextMeshPositionLinkedList.CopyTo(positionArray, 0);
        for (int i = positionArray.Length - 1; i > -1; --i) {
            maxIndex = level - Mathf.Abs(positionArray[i].z);
            if (positionArray[i].x < 1 || positionArray[i].x > maxIndex || positionArray[i].y < 1 || positionArray[i].y > maxIndex) {//出界排除
                nextMeshPositionLinkedList.Remove(positionArray[i]);
            }
        }
    }
    void OnNormalMaterial() {
        Color current = meshRenderer.material.color;
        meshRenderer.material = normalMaterial;
        meshRenderer.material.color = current;
        if (isWaving) {
            myTargetColor = normalMaterial.color;
        } else {
            targetColor = normalMaterial.color;
        }
        gameObject.layer = 9;
    }
    void OnHighlightMaterial() {
        if (HighlightMeshFX) {
            Color current = meshRenderer.material.color;
            meshRenderer.material = highlightMaterial;
            meshRenderer.material.color = current;
            if (isWaving) {
                myTargetColor = highlightMaterial.color;
            } else {
                targetColor = highlightMaterial.color;
            }
        }
        gameObject.layer = 10;
    }
    void OnAttackMaterial() {
        if (AttackMeshFX) {
            Color current = meshRenderer.material.color;
            meshRenderer.material = attackMaterial;
            meshRenderer.material.color = current;
            if (isWaving) {
                myTargetColor = attackMaterial.color;
            } else {
                targetColor = attackMaterial.color;
            }
        }
        gameObject.layer = 10;
    }
    void OnSelectedMaterial() {
        if (SelectedMeshFX) {
            Color current = meshRenderer.material.color;
            meshRenderer.material = selectedMaterial;
            meshRenderer.material.color = current;
            if (isWaving) {
                myTargetColor = selectedMaterial.color;
            } else {
                targetColor = selectedMaterial.color;
            }
        }
        gameObject.layer = 11;
    }
    void OnStartMaterial() {
        if (StartMeshFX) {
            Color current = meshRenderer.material.color;
            meshRenderer.material = startMaterial;
            meshRenderer.material.color = current;
            if (isWaving) {
                myTargetColor = startMaterial.color;
            } else {
                targetColor = startMaterial.color;
            }
        }
        gameObject.layer = 9;
    }
    void LateUpdate() {
        if (ChessManager.chessboardVisible==false &&( meshRenderer.material.name == "BackGroundNormalWhite (Instance)" || meshRenderer.material.name == "BackGroundNormalBlack (Instance)") && UIManager.UIM.chessPieceDisplay) {
            meshRenderer.enabled = false;
        } else {
            meshRenderer.enabled = true;
        }
        if (isDisappear == true) {
            meshRenderer.enabled = false;
            if (Time.time - lastCallTime > timeInterval) {
                meshRenderer.enabled = true;
                isDisappear = false;
            }
        }
    }
    void Disappear() {
        if (isDisappear == false) {
            isDisappear = true;
            meshRenderer.enabled = false;
        }
        lastCallTime = Time.time;
    }
}
