using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class SnakeManagerInServer : NetworkBehaviour {//为了解决在服务器调用客户端主权的函数
    //public LinkedList<NetworkInstanceId> bodyNetIdLinkedList;
    public Dictionary<NetworkInstanceId, LinkedList<NetworkInstanceId>> snakeDictionary = new Dictionary<NetworkInstanceId, LinkedList<NetworkInstanceId>>();
    [Command]
    public void CmdAddSnake(NetworkInstanceId netId) {
        if (snakeDictionary.ContainsKey(netId)) {
            print("AddSnakeBUG");
            return;
        }
        snakeDictionary.Add(netId, new LinkedList<NetworkInstanceId>());
    }
    [Command]
    public void CmdRemoveSnake(NetworkInstanceId netId) {
        if (snakeDictionary.ContainsKey(netId)) {
            snakeDictionary.Remove(netId);
        } else {
            print("RemoveSnakeBUG");
        }
    }
    [Command]
    public void CmdAddBody(NetworkInstanceId snakeNetId, NetworkInstanceId bodyNetId) {
        if (snakeDictionary.ContainsKey(snakeNetId)) {
            LinkedList<NetworkInstanceId> bodyNetIdLinkedList = snakeDictionary[snakeNetId];
            if (bodyNetIdLinkedList.Contains(bodyNetId) == false) {
                bodyNetIdLinkedList.AddLast(bodyNetId);
            } else {
                print("AddBodyBUG");
            }
        } else {
            print("AddBodyBug:NoSnake");
        }
    }
    [Command]
    public void CmdRemoveBody(NetworkInstanceId snakeNetId, NetworkInstanceId bodyNetId) {
        if (snakeDictionary.ContainsKey(snakeNetId)) {
            LinkedList<NetworkInstanceId> bodyNetIdLinkedList = snakeDictionary[snakeNetId];
            if (bodyNetIdLinkedList.Contains(bodyNetId)) {
                bodyNetIdLinkedList.Remove(bodyNetId);
            } else {
                print("CmdRemoveBodyBug");
            }
        } else {
            print("RemoveBodyBUG: NoSnake");
        }
    }
    public NetworkInstanceId GetSnakeNetId(NetworkInstanceId bodyNetId) {
        NetworkInstanceId netId = new NetworkInstanceId((uint)0);
        foreach (KeyValuePair<NetworkInstanceId, LinkedList<NetworkInstanceId>> keyValuePair in snakeDictionary) {
            if (keyValuePair.Value.Contains(bodyNetId)) {
                netId = keyValuePair.Key;
                break;
            }
        }
        if (netId == new NetworkInstanceId((uint)0)) {
            print("GetSnakeNetIdBug");
        }
        return netId;
    }
    [Command]
    public void CmdPrint(NetworkInstanceId netId) {
        print("InCmdPrint"+netId);
        foreach (NetworkInstanceId bodyNetId in snakeDictionary[netId]) {
            print(bodyNetId);
        }
    }
    [Command]
    public void CmdTakeDamageInServer(NetworkInstanceId netId,float damageValue) {
        GameObject snake = NetworkServer.FindLocalObject(GetSnakeNetId(netId));
        print(snake);
        snake.GetComponent<Snake>().CmdTakeDamage(damageValue);
    }
}
