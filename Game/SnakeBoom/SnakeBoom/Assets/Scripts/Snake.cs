using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Snake : NetworkBehaviour {
    public LinkedList<GameObject> snakeLinkedList = new LinkedList<GameObject>();
    public bool isBooming;
    public bool canBoom;
    public GameObject snakeHeadGO;
    public GameObject snakeBodyGO;
    public bool isHungry;//防止一次吃太多造成同步失败

    private Slider sliderForce;
    private Slider sliderHP;
    private Text textIPAddress;
    private Text textSpeed;
    private Text textAttack;
    private float pingPangValue;
    private float timer;
    private GameObject myClientCamera;
    private bool powerOfDragon;
    [SyncVar] private float speed;
    [SyncVar] private float damageValue;
    [SyncVar] private float hp;
    [SyncVar] private uint snakeHeadNetId;
    [SyncVar] private uint snakeBodyNetId;
    [SyncVar] private uint snakeBodySecondNetId;
    [SyncVar] private GameObject boomGameObject;
    //[SyncVar] uint snakeNetId;//不知道为什么用不上
    public override void OnStartServer() {//在Awake之后在Start之前调用,是在客户端的代码，只有NetworkServer.SpawnWithClientAuthority是在服务器执行类似于Instan。。的函数
        //snakeNetId = gameObject.GetComponent<NetworkIdentity>().netId.Value;
        GameObject snakeHead = Instantiate(snakeHeadGO, transform.position + new Vector3(0, 3.62f, -1.3f), transform.rotation) as GameObject;
        GameObject snakeBody = Instantiate(snakeBodyGO, transform.position + new Vector3(0, 3.1f, -2.81f), transform.rotation) as GameObject;
        GameObject snakeBodySecond = Instantiate(snakeBodyGO, transform.position + new Vector3(0, 3.1f, -4.34f), transform.rotation) as GameObject;

        NetworkServer.SpawnWithClientAuthority(snakeHead, connectionToClient);
        snakeHeadNetId = snakeHead.GetComponent<NetworkIdentity>().netId.Value;
        NetworkServer.SpawnWithClientAuthority(snakeBody, connectionToClient);
        snakeBodyNetId = snakeBody.GetComponent<NetworkIdentity>().netId.Value;
        NetworkServer.SpawnWithClientAuthority(snakeBodySecond, connectionToClient);
        snakeBodySecondNetId = snakeBodySecond.GetComponent<NetworkIdentity>().netId.Value;

        hp = 100f;
        damageValue = 5f;
    }
    // [ClientRpc]
    void SetSnake() {//总之在还有乱七八糟的bug的情况下，解决了多段蛇身，在本地联合，在其他服务器和客户端同步的效果！
        if (!isLocalPlayer)
            return;
        GameObject snakeHead = null, snakeBody = null, snakeBodySecond = null/*, snakeGameObject = null*/;
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Body");
        foreach (GameObject element in gos) {//通过id同步物体，怎么不直接同步物体？因为[SynVar]GameObject的localAuthority的物体的id在服务器是0？？现在还不知道怎么回事
            uint value = element.GetComponent<NetworkIdentity>().netId.Value;
            if (value == snakeHeadNetId) {
                snakeHead = element;
            } else if (value == snakeBodyNetId) {
                snakeBody = element;
            } else if (value == snakeBodySecondNetId) {
                snakeBodySecond = element;
            }
            //else if (value == snakeNetId) {//为什么找不到呢？id对啊，反正这里直接调gameObject就行了。。迷
        }
        snakeLinkedList.AddLast(snakeHead);
        snakeLinkedList.AddLast(snakeBody);
        snakeLinkedList.AddLast(snakeBodySecond);
        myClientCamera = GameObject.Find("Main Camera");
        myClientCamera.GetComponent<CameraFollow>().SetTarget(snakeLinkedList.First.Value);
        GameObject snakePart = snakeLinkedList.First.Value;
        snakePart.GetComponent<SnakeHead>().myClientCamera = myClientCamera;
        snakePart.GetComponent<SnakeHead>().mySnake = gameObject;
        snakePart.GetComponent<SnakeBody>().mySnake = gameObject;
        snakePart = snakeLinkedList.First.Next.Value;
        snakePart.GetComponent<SnakeBody>().mySnake = gameObject;
        snakePart = snakeLinkedList.First.Next.Next.Value;
        snakePart.GetComponent<SnakeBody>().mySnake = gameObject;
        snakeBody.SendMessage("SetTargetTf", snakeHead);
        snakeBodySecond.SendMessage("SetTargetTf", snakeBody);
    }
    [Command]
    void CmdCommitSnakeBody(NetworkInstanceId sendNetId) {
        GameObject[] otherSnakes = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject element in otherSnakes) {
            if (sendNetId == element.GetComponent<NetworkIdentity>().netId)//在服务器不找本身映像
                continue;
            element.GetComponent<Snake>().RpcCommitSnakeBody(sendNetId);
        }
    }
    [ClientRpc]
    void RpcCommitSnakeBody(NetworkInstanceId sendNetId) {
        if (!isLocalPlayer) {//只有自己 
            return;
        }
        print("RpcCommitSnakeBody");
        NetworkInstanceId[] netIds = new NetworkInstanceId[snakeLinkedList.Count];
        LinkedListNode<GameObject> p = snakeLinkedList.First;
        for (int i = 0; i < snakeLinkedList.Count; ++i) {
            if (i != 0) {
                p = p.Next;
            }
            netIds[i] = p.Value.GetComponent<NetworkIdentity>().netId;
            print(netIds[i]);
        }
        CmdUpdateSnakeBody(sendNetId, netIds);
    }
    [Command]
    void CmdUpdateSnakeManager() {
        GameObject snakeManagerInServer = GameObject.FindWithTag("SnakeManagerInServer");
        SnakeManagerInServer snakeManager = snakeManagerInServer.GetComponent<SnakeManagerInServer>();
        NetworkInstanceId thisSnakeNetId = GetComponent<NetworkIdentity>().netId;
        snakeManager.CmdAddSnake(thisSnakeNetId);
        snakeManager.CmdAddBody(thisSnakeNetId, new NetworkInstanceId((uint)snakeHeadNetId));
        snakeManager.CmdAddBody(thisSnakeNetId, new NetworkInstanceId((uint)snakeBodyNetId));
        snakeManager.CmdAddBody(thisSnakeNetId, new NetworkInstanceId((uint)snakeBodySecondNetId));
    }
    [Command]
    void CmdSnakeManagerPrint() {
        print("CallCmdPrint");
        GameObject snakeManagerInServer = GameObject.FindWithTag("SnakeManagerInServer");
        SnakeManagerInServer snakeManager = snakeManagerInServer.GetComponent<SnakeManagerInServer>();
        NetworkInstanceId thisSnakeNetId = GetComponent<NetworkIdentity>().netId;
        snakeManager.CmdPrint(thisSnakeNetId);
    }
    [Command]
    void CmdUpdateSnakeBody(NetworkInstanceId sendNetId, NetworkInstanceId[] netIds) {
        NetworkServer.FindLocalObject(sendNetId).GetComponent<Snake>().RpcUpdateSnakeBody(netIds);
    }
    [ClientRpc]
    void RpcUpdateSnakeBody(NetworkInstanceId[] netIds) {
        if (!isLocalPlayer) {
            return;
        }
        if (netIds.Length == 0 || netIds[0] == snakeLinkedList.First.Value.GetComponent<NetworkIdentity>().netId) {
            print(snakeLinkedList.First.Value.GetComponent<NetworkIdentity>().netId);
            return;
        }
        GameObject ob;
        for (int i = 0; i < netIds.Length; ++i) {
            ob = ClientScene.FindLocalObject(netIds[i]);
            if (i == 0) {
                ob.transform.localScale = Vector3.one * 1.5f;
            } else {
                ob.transform.localScale = Vector3.one;
            }
            if (ob.tag == "BoomUnCaptured") {
                ob.tag = "BoomCaptured";
            }
            if (ob.GetComponent<SnakeBody>() == null) {
                ob.AddComponent<SnakeBody>();
            }
        }
    }

    private void Start() {
        if (!isLocalPlayer)
            return;
        SetSnake();
        speed = 7f;
        damageValue = 5f;
        sliderHP = GameObject.Find("Slider - HP").GetComponent<Slider>();
        sliderForce = GameObject.Find("Slider - Force").GetComponent<Slider>();
        textIPAddress = GameObject.Find("Text - IP").GetComponent<Text>();
        textIPAddress.text = "IP地址： " + Network.player.ipAddress;
        textSpeed = GameObject.Find("Text - Speed").GetComponent<Text>();
        textSpeed.text = "速度： " + speed;
        textAttack = GameObject.Find("Text - Attack").GetComponent<Text>();
        textAttack.text = "攻击： " + damageValue;
        sliderForce.gameObject.SetActive(false);
        isBooming = false;
        isHungry = true;
        powerOfDragon = false;
        CmdCommitSnakeBody(gameObject.GetComponent<NetworkIdentity>().netId);
        CmdUpdateSnakeManager();
    }
    private void Update() {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.F)) {
            CmdSnakeManagerPrint();
        }
        if (Input.GetKeyDown(KeyCode.Q)) {
            //SwitchHeadBody();//改
        }
        if (Input.GetKeyUp(KeyCode.C)) {
            if (isBooming == true) {
                RemoveSnakeFirstHead(pingPangValue * 800f + 400f);
                sliderForce.gameObject.SetActive(false);
            }
        }
        if (Input.GetKey(KeyCode.C)) {
            timer += Time.deltaTime;
            pingPangValue = Mathf.PingPong(timer, 1f);
            sliderForce.value = pingPangValue;
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            if (isBooming == false) {//扔炸弹
                if (snakeLinkedList.Count >= 2 && isHungry == true) {
                    GameObject head = snakeLinkedList.First.Value;
                    switch (head.tag) {
                        case "BoomCaptured":
                            isBooming = true;
                            break;
                        case "Body":
                            isBooming = true;
                            break;
                    }
                }
                if (isBooming == true) {
                    pingPangValue = 0f;
                    timer = 0f;
                    sliderForce.gameObject.SetActive(true);
                    sliderForce.value = pingPangValue;
                }
            } else {//引爆炸弹
                if (canBoom == false)
                    return;
                canBoom = false;
                isBooming = false;
                if (boomGameObject == null) {
                    return;
                }
                switch (boomGameObject.tag) {
                    case "BoomCaptured":
                        CmdBoomDuang(true);
                        break;
                    case "Body":
                        CmdBoomDuang(false);
                        break;
                }
            }
        }
    }
    [Command]
    void CmdBoomDuang(bool isBoom) {
        if (boomGameObject == null) {
            print("NoBoom!");
            return;
        }
        if (isBoom) {
            boomGameObject.GetComponent<ThrowBoom>().CmdBoom(true);//服务器创建，服务器销毁
        } else {
            boomGameObject.GetComponent<ThrowBoom>().CmdBounceQuad();//服务器创建,服务器销毁
        }
        boomGameObject = null;
    }
    [Command]
    private void CmdSetBoomCaptured(NetworkInstanceId netIdBoomCaptured) {//被赋权的物体居然只能通过这种方式同步了？客户端服务器都不能同步￥%……&
        RpcSetBoomCaptured(netIdBoomCaptured);
    }
    [ClientRpc]
    private void RpcSetBoomCaptured(NetworkInstanceId netIdBoomCaptured) {
        GameObject boomCaptured = ClientScene.FindLocalObject(netIdBoomCaptured);
        boomCaptured.tag = "BoomCaptured";//我敢说，服务器拥有永久控制权，所有客户端执行 怎么啪啪打脸- -
        boomCaptured.transform.localScale = Vector3.one * 1.5f;
    }
    [Command]
    private void CmdSetScale(NetworkInstanceId netIdGo, Vector3 scale) {
        RpcSetScale(netIdGo, scale);
    }
    [ClientRpc]
    private void RpcSetScale(NetworkInstanceId netIdGo, Vector3 scale) {
        GameObject go = ClientScene.FindLocalObject(netIdGo);
        go.transform.localScale = scale;
    }
    [Command]
    public void CmdCreatBoom(NetworkInstanceId netIdRemoveBoom, Vector3 position, Quaternion rotation) {//所有代码的Command 和 Client 的形参 重写
        GameObject removeBoom = NetworkServer.FindLocalObject(netIdRemoveBoom);
        GameObject boom = Resources.Load("BoomWithAuthority") as GameObject;
        GameObject boomCaptured = Instantiate(boom, position, rotation);
        NetObjectId.Objects.Remove(removeBoom);
        Destroy(removeBoom);
        NetworkServer.SpawnWithClientAuthority(boomCaptured, connectionToClient);
        RpcAddSnakeFirstHead(boomCaptured.GetComponent<NetworkIdentity>().netId);
    }
    [ClientRpc]
    public void RpcAddSnakeFirstHead(NetworkInstanceId netIdOb) {//假设传过来的就是客户端的go映像，此时都有authority，就当单机编就行,其他客户端不能同步
        if (!isLocalPlayer)
            return;
        CmdSetBoomCaptured(netIdOb);//因为hasAuthority，我还不知道能不能完美同步，所以就除了transform就广播信息吧,不能同步，不能在服务器同步，只能广播客户端
        GameObject ob = ClientScene.FindLocalObject(netIdOb);
        CmdSetScale(snakeLinkedList.First.Value.GetComponent<NetworkIdentity>().netId, Vector3.one);
        GameObject lastFirst = snakeLinkedList.First.Value;
        snakeLinkedList.AddFirst(ob);
        Destroy(lastFirst.GetComponent<SnakeHead>());
        lastFirst.SendMessage("SetTargetTf", ob);
        if (ob.GetComponent<SnakeHead>() == null) {
            SnakeHead sh = ob.AddComponent<SnakeHead>();
            sh.myClientCamera = myClientCamera;
            sh.mySnake = gameObject;
        }
        if (ob.GetComponent<SnakeBody>() == null) {
            SnakeBody sb = ob.AddComponent<SnakeBody>();
            sb.mySnake = gameObject;
        }
        if (ob.GetComponent<ExplosionForce>() == null)
            ob.AddComponent<ExplosionForce>();
        myClientCamera.SendMessage("SetTarget", ob);
        if (powerOfDragon) {
            snakeLinkedList.First.Value.GetComponent<SnakeHead>().SetThePowerOfDragon(powerOfDragon);
        }
        CmdAddSnakeBodyInManager(snakeLinkedList.First.Value.GetComponent<NetworkIdentity>().netId);
        isHungry = true;
    }
    [Command]
    void CmdAddSnakeBodyInManager(NetworkInstanceId bodyNetId) {
        GameObject snakeManagerInServer = GameObject.FindWithTag("SnakeManagerInServer");
        SnakeManagerInServer snakeManager = snakeManagerInServer.GetComponent<SnakeManagerInServer>();
        NetworkInstanceId thisSnakeNetId = GetComponent<NetworkIdentity>().netId;
        snakeManager.CmdAddBody(thisSnakeNetId, bodyNetId);
    }
    [Command]
    void CmdRemoveSnakeBodyInManager(NetworkInstanceId bodyNetId) {
        GameObject snakeManagerInServer = GameObject.FindWithTag("SnakeManagerInServer");
        SnakeManagerInServer snakeManager = snakeManagerInServer.GetComponent<SnakeManagerInServer>();
        NetworkInstanceId thisSnakeNetId = GetComponent<NetworkIdentity>().netId;
        snakeManager.CmdRemoveBody(thisSnakeNetId, bodyNetId);
    }
    //[Command]
    //void CmdSetThrowBoom(NetworkInstanceId netId) {
    //    RpcSetThrowBoom(netId);
    //}
    //[ClientRpc]
    //void RpcSetThrowBoom(NetworkInstanceId netId) {
    //    if (isLocalPlayer)
    //        return;
    //    print(connectionToClient);
    //    GameObject ob = ClientScene.FindLocalObject(netId);
    //    SnakeHead sh = ob.GetComponent<SnakeHead>();
    //    if (sh != null) {
    //        Destroy(sh);
    //    }
    //    SnakeBody sb = ob.GetComponent<SnakeBody>();
    //    if (sb != null) {
    //        Destroy(sb);
    //    }
    //    ThrowBoom tb = ob.AddComponent<ThrowBoom>();
    //}
    [Command]
    void CmdCreateThrowBoom(Vector3 position, Quaternion rotation, Vector3 forward, Vector3 right, float force, bool isBoom) {
        GameObject prefab = null;
        if (isBoom) {
            prefab = Resources.Load("BoomInTheAir") as GameObject;
        } else {
            prefab = Resources.Load("QuadInTheAir") as GameObject;
        }
        GameObject lastFirst = Instantiate(prefab, position, rotation);
        //print("damamgValue" + damageValue);
        lastFirst.SendMessage("CmdSetDamageValue", damageValue);
        lastFirst.SendMessage("CmdSetForwardDirection", forward);
        lastFirst.SendMessage("CmdSetRightDirection", right);
        lastFirst.SendMessage("CmdThrow", force);
        NetworkServer.Spawn(lastFirst);
        boomGameObject = lastFirst;
    }
    [Command]
    void CmdDestroyInServer(NetworkInstanceId netId) {
        GameObject go = NetworkServer.FindLocalObject(netId);
        Destroy(go);
    }
    private void RemoveSnakeFirstHead(float force) {
        if (!isLocalPlayer)
            return;
        if (snakeLinkedList.Count < 2)
            return;
        isHungry = false;
        GameObject lastFirst = snakeLinkedList.First.Value;
        snakeLinkedList.RemoveFirst();
        CmdRemoveSnakeBodyInManager(lastFirst.GetComponent<NetworkIdentity>().netId);
        Vector3 position = lastFirst.transform.position;
        Quaternion rotation = lastFirst.transform.rotation;
        bool isBoom = lastFirst.tag == "BoomCaptured" ? true : false;
        CmdDestroyInServer(lastFirst.GetComponent<NetworkIdentity>().netId);
        CmdCreateThrowBoom(position, rotation, myClientCamera.transform.forward, myClientCamera.transform.right, force, isBoom);
        //CmdSetThrowBoom(lastFirst.GetComponent<NetworkIdentity>().netId);

        //SnakeHead sh = lastFirst.GetComponent<SnakeHead>();
        //if (sh != null) {
        //    Destroy(sh);
        //}
        //SnakeBody sb = lastFirst.GetComponent<SnakeBody>();
        //if (sb != null) {
        //    Destroy(sb);
        //}
        //ThrowBoom tb = lastFirst.AddComponent<ThrowBoom>();

        //CmdRemoveAuthority(boomGameObject.GetComponent<NetworkIdentity>().netId);

        GameObject newFirst = snakeLinkedList.First.Value;
        newFirst.SendMessage("CancelSetTargetTf");
        SnakeHead shh = newFirst.GetComponent<SnakeHead>();
        if (shh == null) {
            shh = newFirst.AddComponent<SnakeHead>();
        }
        shh.myClientCamera = myClientCamera;
        shh.mySnake = gameObject;
        CmdSetScale(newFirst.GetComponent<NetworkIdentity>().netId, Vector3.one * 1.5f);
        myClientCamera.SendMessage("SetTarget", newFirst);
        if (powerOfDragon) {
            snakeLinkedList.First.Value.GetComponent<SnakeHead>().SetThePowerOfDragon(powerOfDragon);
        }
        isHungry = true;
        canBoom = true;
    }

    [Command]
    void CmdRemoveAuthority(NetworkInstanceId netId) {
        GameObject go = NetworkServer.FindLocalObject(netId);
        NetworkIdentity goID = go.GetComponent<NetworkIdentity>();
        if (goID.clientAuthorityOwner != null) {
            print("OWner:" + goID.clientAuthorityOwner);
            goID.RemoveClientAuthority(connectionToClient);
        }
    }
    private void SwitchHeadBody() {
        if (snakeLinkedList.Count < 2)
            return;
        GameObject lastFirst = snakeLinkedList.First.Value;
        snakeLinkedList.RemoveFirst();
        GameObject lastSecond = snakeLinkedList.First.Value;
        snakeLinkedList.RemoveFirst();
        Destroy(lastFirst.GetComponent<SnakeHead>());
        if (lastSecond.GetComponent<SnakeHead>() == null)
            lastSecond.AddComponent<SnakeHead>();
        lastFirst.GetComponent<SnakeBody>().forwardSnakeBody = lastSecond;
        lastFirst.transform.localScale = Vector3.one;
        lastSecond.transform.localScale = Vector3.one * 1.5f;
        Vector3 p = lastFirst.transform.position;
        lastFirst.transform.position = lastSecond.transform.position;
        lastSecond.transform.position = p;
        snakeLinkedList.AddFirst(lastFirst);
        snakeLinkedList.AddFirst(lastSecond);
        lastSecond.SendMessage("CancelSetTargetTf");
        GameObject[] gameObjectArray = new GameObject[snakeLinkedList.Count];
        snakeLinkedList.CopyTo(gameObjectArray, 0);
        for (int i = 1; i < gameObjectArray.Length; ++i) {
            gameObjectArray[i].SendMessage("SetTargetTf", gameObjectArray[i - 1]);
        }
        myClientCamera.SendMessage("SetTarget", lastSecond);
    }
    [ClientRpc]
    private void RpcChangeHP() {
        if (!isLocalPlayer)
            return;
        sliderHP.value = hp;
        print(hp);
    }
    [ClientRpc]
    private void RpcChangeSpeed() {
        if (!isLocalPlayer)
            return;
        textSpeed.text = "速度： " + speed;
    }
    [ClientRpc]
    private void RpcChangeAttack() {
        if (!isLocalPlayer)
            return;
        textAttack.text = "攻击： " + damageValue;
    }
    [Command]
    public void CmdTakeDamage(float damageValue) {
        print("CmdTakeDamage");
        hp -= damageValue;
        if (hp < 0) {
            hp = 0;
        }
        if (hp == 0) {
            RpcGoDie();
        } else {
            RpcChangeHP();
        }
    }
    [Command]
    public void CmdSetFireFX(Vector3 position, Quaternion rotation) {
        GameObject.FindWithTag("DragonNest").GetComponent<ThePowerOfDragonManager>().SetFireFX(position, rotation);
    }
    [ClientRpc]
    void RpcGoDie() {
        if (!isLocalPlayer)
            return;
        if (powerOfDragon == true) {
            GameObject dragonNest = GameObject.FindWithTag("DragonNest");
            CmdSetFireFX(dragonNest.transform.position, dragonNest.transform.rotation);
            powerOfDragon = false;
            snakeLinkedList.First.Value.SendMessage("SetThePowerOfDragon", false);
            CmdSetWhoGet(new NetworkInstanceId((uint)0));
        }
        GameObject head = snakeLinkedList.First.Value;
        head.transform.position = new Vector3(Random.Range(-70, 70), 13f, Random.Range(-70, 70));
        hp = 100;
        sliderHP.value = hp;
    }
    [Command]
    public void CmdEatAttackFu(float damageValuePlus) {
        damageValue += damageValuePlus;
        if (damageValue > 50f)
            damageValue = 50f;
        RpcChangeAttack();
    }
    public void EatSpeedFu(float speedPlus) {
        if (!isLocalPlayer)
            return;
        foreach (GameObject element in snakeLinkedList) {
            SnakeHead sh = element.GetComponent<SnakeHead>();
            if (sh != null) {
                sh.AddSpeed(speedPlus);
            }
            SnakeBody sb = element.GetComponent<SnakeBody>();
            if (sb != null) {
                sb.AddSpeed(speedPlus);
            }
        }
        speed += speedPlus;
        if (speed > 15f)
            speed = 15f;
        RpcChangeSpeed();
    }
    [Command]
    public void CmdEatRecoveryFu() {
        hp = 100f;
        RpcChangeHP();
    }
    [ClientRpc]
    public void RpcOwnThePowerOfDragon(NetworkInstanceId netId) {
        if (!isLocalPlayer)
            return;
        if (netId == GetComponent<NetworkIdentity>().netId) {
            powerOfDragon = true;
            snakeLinkedList.First.Value.SendMessage("SetThePowerOfDragon", true);
        } else {
            powerOfDragon = false;
            snakeLinkedList.First.Value.SendMessage("SetThePowerOfDragon", false);
        }
    }
    [Command]
    public void CmdSetWhoGet(NetworkInstanceId whoGetNetId) {
        GameObject.FindWithTag("DragonNest").GetComponent<ThePowerOfDragonManager>().whoGet = whoGetNetId;
    }
    [Command]
    public void CmdGiveThePowerOfDragon(NetworkInstanceId netId) {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject element in players) {
            element.GetComponent<Snake>().RpcOwnThePowerOfDragon(netId);
        }
    }
    [ClientRpc]
    private void RpcOwnThePowerOfDragonBool(bool isOwn) {
        if (!isLocalPlayer)
            return;
        powerOfDragon = isOwn;
        snakeLinkedList.First.Value.SendMessage("SetThePowerOfDragon", powerOfDragon);
    }
}
