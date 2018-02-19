using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    public GameObject target;
    public float viewMoveSpeed;
    private SphericalVector sphericalVector = new SphericalVector(6f, 1f, 0.3f);
    private void Start() {
        SetTarget(target);
    }
    private void Update() {
        if (target == null)
            return;
        float h = Input.GetAxis("Mouse X");//水平视角移动
        float v = Input.GetAxis("Mouse Y");//垂直视角移动
        sphericalVector.azimuth += h * viewMoveSpeed;
        sphericalVector.zenith -= v * viewMoveSpeed;
        sphericalVector.zenith = Mathf.Clamp(sphericalVector.zenith, 0f, 1f);
        float s = Input.GetAxis("Mouse ScrollWheel");//滚轮拉近视角
        sphericalVector.length -= s * 10f;
        sphericalVector.length = Mathf.Clamp(sphericalVector.length, 2f, 25f);
        transform.position = target.transform.position + sphericalVector.Position;//设定摄像机位置
        transform.LookAt(target.transform);//摄像机视角
    }
    public void SetTarget(GameObject target) {
        this.target = target;
    }
}
