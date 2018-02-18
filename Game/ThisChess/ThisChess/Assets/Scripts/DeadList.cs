using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadList : MonoBehaviour {
    public bool isWhite;
    public List<GameObject> deads = new List<GameObject>();
    public Vector3 Position { get; set; }
    public Vector3 Direction {
        get { return direction; }
        set { direction = value.normalized; }
    }
    public float step;

    private Vector3 direction;
    private void Add(GameObject dead) {
        deads.Add(dead);
        if (deads.Count != 1)
            Position += Direction * step;
       dead.layer = 0;//层次为0 就不会被点到了
    }
    private void Remove(GameObject alive) {
        deads.Remove(alive);
        if (deads.Count != 0)
            Position -= Direction * step;
        alive.layer = 8;//层次为8 重新成为Player
    }
}
