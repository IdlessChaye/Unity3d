using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterTween : MonoBehaviour {
    public void ActiveFlase() {
        UILabel lb = GetComponent<UILabel>();
        lb.color = new Color(lb.color.r, lb.color.g, lb.color.b, 1);
        GetComponent<TweenAlpha>().enabled = false;
        GetComponent<TweenAlpha>().enabled = true;
        gameObject.SetActive(false);
    }
}
