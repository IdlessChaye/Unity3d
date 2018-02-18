using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSetting : MonoBehaviour {
    public void CheckCheckUI() {
        ChessManager.CheckUI = UIToggle.current.value;
    }
}
