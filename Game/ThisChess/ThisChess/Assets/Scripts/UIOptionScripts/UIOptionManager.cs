using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIOptionManager : MonoBehaviour {
    private static UIOptionManager uiOptionManager;
    public static UIOptionManager UIOM {
        get {
            return uiOptionManager;
        }
    }
    public GameObject spriteButtonGame;
    public GameObject spriteButtonDisplay;
    public GameObject spriteButtonMusic;
    public GameObject spriteButtonHelp;

    public GameObject panelGame;
    public GameObject panelDisplay;
    public GameObject panelMusic;
    public GameObject panelHelp;

    private GameObject currentOptionPanel;
    private void Awake() {
        uiOptionManager = this;
    }
    private void Start() {
        currentOptionPanel = panelGame;
    }
    private void StartOption() {
        if(UIManager.panelReturn.name== "Panel - Game") {//先在显示上回归初始状态
            ChessPieceMoveManager.chessPieceMoveManager.SendMessage("ShutDown");
        }
        SwitchSettingPanel(currentOptionPanel);//BUG选项颜色不同步
    }
    private void DarkSettingSpriteButton() {
        spriteButtonGame.GetComponent<UISprite>().color = Color.black;
        spriteButtonDisplay.GetComponent<UISprite>().color = Color.black;
        spriteButtonMusic.GetComponent<UISprite>().color = Color.black;
        spriteButtonHelp.GetComponent<UISprite>().color = Color.black;
    }
    private void HighlightSettingSpriteButton(GameObject spriteButton) {
        DarkSettingSpriteButton();
        spriteButton.GetComponent<UISprite>().color = Color.white;
    }
    private void ShutDownSettingPanel() {
        panelGame.SetActive(false);
        panelDisplay.SetActive(false);
        panelMusic.SetActive(false);
        panelHelp.SetActive(false);
    }
    private void SwitchSettingPanel(GameObject panel) {
        ShutDownSettingPanel();
        panel.SetActive(true);
        currentOptionPanel = panel;
        switch (panel.name) {
            case "Panel - GameSetting":
                HighlightSettingSpriteButton(spriteButtonGame);
                break;
            case "Panel - DisplaySetting":
                HighlightSettingSpriteButton(spriteButtonDisplay);
                break;
            case "Panel - MusicSetting":
                HighlightSettingSpriteButton(spriteButtonMusic);
                break;
            case "Panel - HelpSetting":
                HighlightSettingSpriteButton(spriteButtonHelp);
                break;
        }
    }
    public void SwitchToGameSettingPanel() {
        SwitchSettingPanel(panelGame);
    }
    public void SwitchToDisplaySettingPanel() {
        SwitchSettingPanel(panelDisplay);
    }
    public void SwitchToMusicSettingPanel() {
        SwitchSettingPanel(panelMusic);
    }
    public void SwitchToHelpettingPanel() {
        SwitchSettingPanel(panelHelp);
    }
}
