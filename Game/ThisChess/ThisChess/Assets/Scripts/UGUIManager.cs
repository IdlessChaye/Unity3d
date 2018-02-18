using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UGUIManager : MonoBehaviour {
    public static UGUIManager uguiManager;
    public static UGUIManager UGUIM {
        get {
            return uguiManager;
        }
    }
    public static bool uguiIsWorking;
    public GameObject UGUICamera;
    public GameObject panelExitGame;
    public GameObject panelQuitGame;
    private void Awake() {
        uguiManager = this;
        uguiIsWorking = false;
    }
    private void Start() {
        OffUGUI(); 
    }
    public void OffUGUI() {//屏蔽UGUI
        ShutDownPanels();
        UGUICamera.SetActive(false);//关闭摄像机
        UIManager.UIM.SendMessage("UIEvent");//开启NGUI事件
        uguiIsWorking = false;
    }
    public void OnUGUI() {//显示UGUI
        UIManager.UIM.SendMessage("NoEvent");//取消NGUI事件
        UGUICamera.SetActive(true);//开启摄像机
    }
    private void ShutDownPanels() {
        panelExitGame.SetActive(false);
        panelQuitGame.SetActive(false);
    }
    private void SwitchToPanel(GameObject panel) {
        ShutDownPanels();
        panel.SetActive(true);
    }
    public void SwitchToExitGame() {
        if (UIManager.panelReturn.name == "Panel - Main") {
            UIManager.UIM.SendMessage("SwitchToPanelMain");
            return;//如果ESC从主菜单诞生，就不需要QuitGame了 ，这算是逻辑BUG的修复吧。。
        }
        uguiIsWorking = true;
        OnUGUI();
        SwitchToPanel(panelExitGame);
    }
    public void SwitchToQuitGame() {
        uguiIsWorking = true;
        OnUGUI();
        SwitchToPanel(panelQuitGame);
    }

}
