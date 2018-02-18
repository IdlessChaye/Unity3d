using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EightMesh : MonoBehaviour {
    //public static EightMesh eightMesh;
    public float l;

    //void start() {
    //    for (int i = 0; i < 3; ++i) {
    //        gameobject go = createeight(l);
    //        go.transform.position = new vector3(-l + i * l, 0, 0);
    //    }
    //}
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
        mesh.triangles = new int[] { 0,4,1,1,4,2,2,4,3,4,0,3,0,1,5,1,2,5,2,3,5,3,0,5};
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        meshFilter.sharedMesh = mesh;
        go.AddComponent<MeshRenderer>();
        go.AddComponent<MeshCollider>();
        return go;
    }


}
