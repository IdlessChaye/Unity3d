using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessboardManager : MonoBehaviour {
    public static Transform mapHolder;
    public float angelSpeed;
    public float Ylength;
    private float myTime;
    public static int Length { get; set; }
    public static int Level { get; set; }
    private void Awake() { //你们终于变成静态变量了
        Length = 10;
        Level = 6;
    }
    void Start() {
        CreateChessboardOfResource(Length, Level);
        myTime = 0;
    }
    public void CreateChessboardOfResource(int length, int level) { //利用Resources创建地图，length为10
        mapHolder = new GameObject("MapHolder").transform;
        GameObject octahedron = Resources.Load("Prefabs/Octahedron") as GameObject;
        float half = (float)length / 2;
        float high = -Mathf.Sqrt(2) * half;
        for (int l = 1; l <= level; ++l) {
            high += Mathf.Sqrt(2) * half;
            float y = -level + l - 2;
            for (int i = 0; i < level + 1 - l; ++i) {
                y += 2;
                float x = -level + l - 2;
                for (int j = 0; j < level + 1 - l; ++j) {
                    x += 2;
                    GameObject go = Instantiate(octahedron, new Vector3(x * half, high, y * half), Quaternion.identity);
                    go.transform.SetParent(mapHolder);
                    Vector3 position = new Vector3(j + 1, i + 1, l - 1);
                    ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary.Add(position, go);
                    go.GetComponent<MeshRendererManager>().Position = position;
                    if (l > 1) {//除了第一层，创两层八面体
                        go = Instantiate(octahedron, new Vector3(x * half, -high, y * half), Quaternion.identity);
                        go.transform.SetParent(mapHolder);
                        position = new Vector3(j + 1, i + 1, 1 - l);
                        ChessPieceMoveManager.chessPieceMoveManager.chessboardDictionary.Add(position, go);
                        go.GetComponent<MeshRendererManager>().Position = position;
                    }
                }
            }
        }
        //四面体的创建
        //GameObject tetrahedron = Resources.Load("Prefabs/Tetrahedron") as GameObject;
        //high = -Mathf.Sqrt(2) * half;
        //for (int l = 1; l <= level - 1; ++l) {
        //    high += Mathf.Sqrt(2) * half;
        //    float y = -level + l - 2;
        //    for (int i = 0; i < level + 1 - l; ++i) {
        //        y += 2;
        //        float x = -level + l - 1;
        //        for (int j = 0; j < level - l; ++j) {
        //            x += 2;
        //            Instantiate(tetrahedron, new Vector3(x * half, high, y * half), Quaternion.identity).transform.SetParent(mapHolder);

        //            Instantiate(tetrahedron, new Vector3(x * half, -high, y * half), Quaternion.AngleAxis(180, Vector3.forward)).transform.SetParent(mapHolder);

        //            Instantiate(tetrahedron, new Vector3(y * half, high, x * half), Quaternion.AngleAxis(-90, Vector3.up)).transform.SetParent(mapHolder);

        //            Instantiate(tetrahedron, new Vector3(y * half, -high, x * half), Quaternion.Euler(0f,-90f, 180f)).transform.SetParent(mapHolder);
        //        }
        //    }
        //}
        mapHolder.gameObject.layer = 9;
    }
    private void Update() {
        if (UIManager.UIM.MapHolderRotate) {
            myTime += Time.deltaTime;
            mapHolder.transform.Rotate(Vector3.up, -angelSpeed);
            mapHolder.transform.position = new Vector3(mapHolder.transform.position.x, Mathf.PingPong(myTime * Ylength / 2, Ylength) - Ylength / 2, mapHolder.transform.position.z);
        }
    }
    public void CreateMap(int length, int level) {
        Color color;
        Transform mapHolder = new GameObject("MapHolder").transform;
        float half = length / 2;
        float high = -Mathf.Sqrt(2) * half;
        for (int l = 1; l <= level; ++l) {
            high += Mathf.Sqrt(2) * half;
            float y = -level + l - 2;
            for (int i = 0; i < level + 1 - l; ++i) {
                y += 2;
                float x = -level + l - 2;
                for (int j = 0; j < level + 1 - l; ++j) {
                    x += 2;
                    GameObject eightGO = CreateEight(length);
                    Material material = new Material(Shader.Find("Diffuse"));
                    color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    color.a = 0.1f;
                    material.SetColor("_Color", color);
                    eightGO.GetComponent<MeshRenderer>().sharedMaterial = material;
                    eightGO.transform.position = new Vector3(x * half, high, y * half);
                    eightGO.transform.SetParent(mapHolder);
                    if (l > 1) {
                        GameObject eightGO2 = CreateEight(length);
                        Material material2 = new Material(Shader.Find("Diffuse"));
                        color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                        color.a = 0.1f;
                        material2.SetColor("_Color", color);
                        eightGO2.GetComponent<MeshRenderer>().sharedMaterial = material2;
                        eightGO2.transform.position = new Vector3(x * half, -high, y * half);
                        eightGO2.transform.SetParent(mapHolder);

                    }
                }
            }
        }
        high = -Mathf.Sqrt(2) * half;
        for (int l = 1; l <= level - 1; ++l) {
            high += Mathf.Sqrt(2) * half;
            float y = -level + l - 2;
            for (int i = 0; i < level + 1 - l; ++i) {
                y += 2;
                float x = -level + l - 1;
                for (int j = 0; j < level - l; ++j) {
                    x += 2;
                    GameObject fourGO = CreateFour(length);
                    Material material3 = new Material(Shader.Find("Diffuse"));
                    color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    color.a = 0.1f;
                    material3.SetColor("_Color", color);
                    fourGO.GetComponent<MeshRenderer>().sharedMaterial = material3;
                    fourGO.transform.position = new Vector3(x * half, high, y * half);

                    GameObject fourGO3 = CreateFour(length);
                    fourGO3.transform.Rotate(Vector3.forward * 180);
                    Material material5 = new Material(Shader.Find("Diffuse"));
                    color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    color.a = 0.1f;
                    material5.SetColor("_Color", color);
                    fourGO3.GetComponent<MeshRenderer>().sharedMaterial = material5;
                    fourGO3.transform.position = new Vector3(x * half, -high, y * half);

                    GameObject fourGO2 = CreateFour(length);
                    fourGO2.transform.Rotate(Vector3.up * -90);
                    Material material4 = new Material(Shader.Find("Diffuse"));
                    color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    color.a = 0.1f;
                    material4.SetColor("_Color", color);
                    fourGO2.GetComponent<MeshRenderer>().sharedMaterial = material4;
                    fourGO2.transform.position = new Vector3(y * half, high, x * half);

                    GameObject fourGO4 = CreateFour(length);
                    fourGO4.transform.Rotate(Vector3.up * -90);
                    fourGO4.transform.Rotate(Vector3.forward * 180);
                    Material material6 = new Material(Shader.Find("Diffuse"));
                    color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    color.a = 0.1f;
                    material6.SetColor("_Color", color);
                    fourGO4.GetComponent<MeshRenderer>().sharedMaterial = material6;
                    fourGO4.transform.position = new Vector3(y * half, -high, x * half);

                    fourGO.transform.SetParent(mapHolder);
                    fourGO2.transform.SetParent(mapHolder);
                    fourGO3.transform.SetParent(mapHolder);
                    fourGO4.transform.SetParent(mapHolder);

                }
            }
        }
        mapHolder.gameObject.layer = 9;
    }

    public GameObject CreateFour(float length) {
        GameObject go = new GameObject();
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        float half = length / 2;
        mesh.vertices = new Vector3[] {new Vector3(0,0,half),
                    new Vector3(0, 0, -half),
                    new Vector3(-half, Mathf.Sqrt(2)*half, 0) ,
                    new Vector3(half, Mathf.Sqrt(2)*half, 0)};
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1, 3, 0, 1, 2, 0, 3 };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
        go.AddComponent<MeshRenderer>();
        go.AddComponent<MeshCollider>();
        go.AddComponent<MeshRendererManager>();
        go.layer = 9;
        return go;
    }
    public GameObject CreateEight(float length) {
        GameObject go = new GameObject();
        MeshFilter meshFilter = go.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        float half = length / 2;
        mesh.vertices = new Vector3[] {new Vector3(-half,0,half),
                    new Vector3(-half, 0, -half),
                    new Vector3(half, 0, -half),
                    new Vector3(half, 0, half),
                    new Vector3(0, Mathf.Sqrt(2) * half, 0) ,
                    new Vector3(0, -Mathf.Sqrt(2) * half, 0)};
        mesh.triangles = new int[] { 0, 4, 1, 1, 4, 2, 2, 4, 3, 4, 0, 3, 0, 1, 5, 1, 2, 5, 2, 3, 5, 3, 0, 5 };
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
        go.AddComponent<MeshRenderer>();
        go.AddComponent<MeshCollider>();
        go.AddComponent<MeshRendererManager>();
        go.layer = 9;
        return go;
    }
}

