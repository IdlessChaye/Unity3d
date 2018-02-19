//这个脚本是控制刷出红色方块用的

using UnityEngine;
using System.Collections;

public class Food : MonoBehaviour
{
    public GameObject SnakeFood;
    public bool IfFood = true;

    private GameObject SF;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (IfFood)
        {
            float x = 18 - Random.value * 36;
            float z = 18 - Random.value * 36;  //Random.value是在0到1之间随机取值，这两行意思是在-18到+18之间随机取值
            SF = Instantiate(SnakeFood, new Vector3(x, 5, z), Quaternion.identity) as GameObject;  //在坐标(x,5,z)处刷出一个Food，在Unity编辑器中我们将Food设置为了红色方块
            IfFood = false;
        }
        else if (SF.transform.position.y < -1)  //如果红方块掉下去了就重新刷出来一个
        {
            Destroy(SF);
            IfFood = true;
        }
    }
}