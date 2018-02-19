using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
public class UINetworkManager : MonoBehaviour {
    private void Start() {
        OfflineUIset();
    }
    public void StartHost() {
        NetworkManager.singleton.StartHost();
    }
    public void StartClient() {
        string ipAddress = GameObject.Find("InputField - IPAddress").GetComponent<InputField>().text;
        if (ipAddress != "") {
            NetworkManager.singleton.networkAddress = ipAddress;
        }
        NetworkManager.singleton.StartClient();
    }
    public void StopHost() {
        NetworkManager.singleton.StopHost();
    }
    void OfflineUIset() {
        GameObject.Find("Button - CreateHost").GetComponent<Button>().onClick.AddListener(StartHost);
        GameObject.Find("Button - LinkTo").GetComponent<Button>().onClick.AddListener(StartClient);
        GameObject.Find("Button - GameExit").GetComponent<Button>().onClick.AddListener(ExitGame);
    }
    void OnlineUIset() {
        GameObject.Find("Button - StopLink").GetComponent<Button>().onClick.AddListener(StopHost);
    }
    private void OnLevelWasLoaded(int level) {
        if (level == 0) {
            OfflineUIset();
        } else {
            OnlineUIset();
        }
    }
    public void ExitGame() {
        Application.Quit(); 
    }
}
