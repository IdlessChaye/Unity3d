using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplaySetting : MonoBehaviour {
    private GameObject UIToggleChessboardVisible;
    private void Start() {
        UIToggleChessboardVisible = GameObject.Find("CheckBox - ChessboardVisible");
    }
    private void Update() {
        ChangeUIChessboardVisible();
    }
    public void CheckChessPieceFX() {
        ChessPieceParticleSystem.ChessPieceFX = UIToggle.current.value;
    }
    public void CheckWaveFX() {
        ChessPieceMoveManager.WaveFX = UIToggle.current.value;
    }
    public void CheckWaveKillFX() {
        ChessPieceMoveManager.WaveKillFX = UIToggle.current.value;
    }
    public void CheckHighlightMeshFX() {
        MeshRendererManager.HighlightMeshFX = UIToggle.current.value;
    }
    public void CheckStartMeshFX() {
        MeshRendererManager.StartMeshFX = UIToggle.current.value;
    }
    public void CheckAttackMeshFX() {
        MeshRendererManager.AttackMeshFX = UIToggle.current.value;
    }
    public void CheckSelectedMeshFX() {
        MeshRendererManager.SelectedMeshFX = UIToggle.current.value;
    }
    public void CheckRemoveStillCheckMeshFX() {
        ChessPieceMoveManager.RemoveStillCheckMeshFX = UIToggle.current.value;
    }
    public void CheckCullingMeshFadeFX() {
        CameraFollow.CullingMeshFadeFX = UIToggle.current.value;
    }
    public void CheckChessboardVisible() {
        ChessManager.chessboardVisible = UIToggle.current.value;
    }
    public void ChangeUIChessboardVisible() {
        UIToggleChessboardVisible.GetComponent<UIToggle>().value = ChessManager.chessboardVisible;
    }
}
