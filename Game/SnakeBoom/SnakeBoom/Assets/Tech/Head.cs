//这个脚本是用来控制蛇的运动和计分系统的

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Head : MonoBehaviour
{
    public GameObject BodyPiece;
    public float HeadSpeed, BodySpeed;

    private Transform Camera;
    private GameObject[] Body = new GameObject[300];  //最多就能吃300个
    private GameObject FoodGenerator;
    private int BodyNumber = 3;
    private Text Score;

    // Use this for initialization
    void Start()
    {
        Camera = GameObject.Find("Camera").GetComponent<Transform>();
        transform.rotation = new Quaternion(0, Camera.rotation.y, 0, Camera.rotation.w);
        Body[0] = GameObject.Find("Body (0)");
        Body[1] = GameObject.Find("Body (1)");
        Body[2] = GameObject.Find("Body (2)");
        FoodGenerator = GameObject.Find("Food Generator");
        Score = GameObject.Find("Score").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = new Quaternion(0, Camera.rotation.y, 0, Camera.rotation.w);  //头的方向和镜头方向一致，但镜头上下转动对头不造成影响
        transform.Translate(Vector3.forward * HeadSpeed * 0.04f);  //蛇头始终朝前动
        for (int i = 1; i < BodyNumber; i++)
        {
            if (Body[i].transform.position.y > -1)
            {
                Body[i].transform.LookAt(Body[i - 1].transform);
                if (Vector3.Distance(Body[i - 1].transform.position, Body[i].transform.position) > 2)
                    Body[i].transform.Translate(Vector3.forward * BodySpeed * 0.02f);  //上面这几行：让后一节身体跟着前一节走
            }
            else
            {
                Destroy(Body[i]);
                BodyNumber--;
                for (int j = i; j < BodyNumber; j++)
                    Body[j] = Body[j + 1];
                Body[BodyNumber] = null;  //上面几行：如果有几节身体掉下去了就让后面的重新接上，并扣分
            }
        }
        Score.text = "Score:" + (BodyNumber - 3).ToString();  //在右上角显示分数
    }

    void OnTriggerEnter(Collider Col)  //这个函数在蛇头碰到了其它碰撞体的时候就会运行
    {
        if (Col.name == "Food(Clone)")  //如果撞到了红色方块就执行下面的代码，即创建一节身体补在最后面
        {
            Destroy(Col.gameObject);
            Body[BodyNumber] = Instantiate(BodyPiece) as GameObject;
            Body[BodyNumber].transform.parent = Body[BodyNumber - 1].transform;
            Body[BodyNumber].transform.localPosition = new Vector3(0, 0, -1.5f);
            Body[BodyNumber].transform.localRotation = new Quaternion(0, 0, 0, 0);
            Body[BodyNumber - 1].transform.DetachChildren();
            BodyNumber++;
            FoodGenerator.GetComponent<Food>().IfFood = true;
        }
    }
}
