using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuManager : MonoBehaviour {
    [SerializeField] public  FuMode thisFuMode;
}
public enum FuMode {
    Speed, Attack, Spawn, Recovery
};
