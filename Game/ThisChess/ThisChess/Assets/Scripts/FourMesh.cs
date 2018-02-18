using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourMesh : MonoBehaviour {
    //public static FourMesh fourMesh;
    public float l;


    
    //private void Start() {
    //    for(int i=0;i<3;++i) {
    //        GameObject go = CreateFour(l);
    //        go.transform.position = new Vector3(-l + i * l, 0, 0);
    //    }
    //}
    public GameObject CreateFour(float length) {
        GameObject go = new GameObject();
        MeshFilter meshFilter=go.AddComponent<MeshFilter>();
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
        return go;
    }
}
