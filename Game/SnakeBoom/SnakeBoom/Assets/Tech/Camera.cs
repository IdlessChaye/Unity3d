//这个脚本是控制镜头用的

using UnityEngine;
using System.Collections;

public class Camera : MonoBehaviour
{
    public float RotateSpeed, AngleLimitUp, AngleLimitDown;  //创建变量，前面有"public"意味着你可以在Unity编辑器里修改它们的值，这三个变量分别是镜头旋转速度、镜头上下倾角最大度数

    private float RotateX, RotateY;  //private则意味着在这个脚本之外不能用或修改这些变量，这两个变量用于累计鼠标移动量
    private Transform Target;  //这个是镜头需要对准的物体的位置信息

	// Use this for initialization  Start()函数用于初始化
	void Start ()
    {
        Target = GameObject.Find("Head").GetComponent<Transform>();  //找到镜头需要对准的物体并获得它的位置信息
        transform.position = Target.position;  //使镜头的支点架在目标上
    }
	
	// Update is called once per frame  Update()函数每一帧都要运行一次
	void Update ()
    {
        transform.position = Target.position;  //每一帧运行时都把镜头支架架在目标上
        RotateX = (RotateX + RotateSpeed * Input.GetAxis("Mouse X")) % 360f;
        RotateY = (RotateY - RotateSpeed * Input.GetAxis("Mouse Y")) % 360f;  //在一帧运行时间内分别累计鼠标在横竖两个方向上的位移
        if (RotateY > AngleLimitUp)
            RotateY = AngleLimitUp;
        if (RotateY < AngleLimitDown)
            RotateY = AngleLimitDown;  //如果角度超过倾角限制则将角度设定为最大值
        transform.rotation = Quaternion.AngleAxis(RotateX, Vector3.up) * Quaternion.AngleAxis(RotateY, Vector3.right);  //Quaternion是四元数，表示旋转，将镜头支架根据鼠标位移进行旋转
    }
}
