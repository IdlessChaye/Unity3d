using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class AudioSample : MonoBehaviour {
    void Update() {
        float[] spectrum = new float[256];

        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        float maxValue = spectrum[1], minValue = spectrum[1];
        for (int i = 1; i < spectrum.Length - 1; i++) {
            if (spectrum[i - 1] > maxValue)
                maxValue = spectrum[i - 1];
            if (spectrum[i - 1] < minValue) {
                minValue = spectrum[i - 1];
            }
            Debug.DrawLine(new Vector3(i - 1, Mathf.Log(spectrum[i - 1]), 2f), new Vector3(i, Mathf.Log(spectrum[i]), 2f), Color.cyan);
        }
        print("Max==" + maxValue + "Min==" + minValue);
    }
}
