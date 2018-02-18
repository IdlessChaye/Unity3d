using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSetting : MonoBehaviour {
    public void SetVolumeValue() {
        print(UISlider.current.value);
        AudioManager.AM.SendMessage("SetVolume", UISlider.current.value);
    }
    public void CheckChooseSound() {
        AudioManager.ChooseSound = UIToggle.current.value;
    }
    public void CheckMoveSound() {
        AudioManager.MoveSound = UIToggle.current.value;
    }
    public void CheckCaptureSound() {
        AudioManager.CaptureSound = UIToggle.current.value;
    }
    public void CheckUnChooseSound() {
        AudioManager.UnChooseSound = UIToggle.current.value;
    }
    public void CheckCantMoveSound() {
        AudioManager.CantMoveSound = UIToggle.current.value;
    }
    public void CheckCheckSound() {
        AudioManager.CheckSound = UIToggle.current.value;
    }
    public void CheckCheckmateSound() {
        AudioManager.CheckmateSound = UIToggle.current.value;
    }
    public void CheckPromoteSound() {
        AudioManager.PromoteSound = UIToggle.current.value;
    }
}
