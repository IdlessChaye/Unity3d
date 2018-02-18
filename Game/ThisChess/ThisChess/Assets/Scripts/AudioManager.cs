using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    private static AudioManager audioManager;
    public static AudioManager AM {
        get {
            return audioManager;
        }
    }
    public AudioClip unity;
    public AudioClip choose;
    public AudioClip unChoose;
    public AudioClip cantMove;
    public AudioClip move;
    public AudioClip capture;
    public AudioClip check;
    public AudioClip checkmate;
    public AudioClip promote;

    private AudioSource audio;

    public static bool ChooseSound { get; set; }
    public static bool UnChooseSound { get; set; }
    public static bool CantMoveSound { get; set; }
    public static bool MoveSound { get; set; }
    public static bool CaptureSound { get; set; }
    public static bool CheckSound { get; set; }
    public static bool CheckmateSound { get; set; }
    public static bool PromoteSound { get; set; }
    private void SetBools() {
        ChooseSound = true;
        UnChooseSound = false;
        CantMoveSound = true;
        MoveSound = true;
        CaptureSound = true;
        CheckSound = true;
        CheckmateSound = true;
        PromoteSound = true;
    }

    void Awake() {
        audioManager = this;
        audio = GetComponent<AudioSource>();
        SetBools();
    }
    void PlayMusic(AudioClip ac) {
        audio.clip = ac;
        audio.Play();
    }
    void PlayChoose() {
        ResetAudioSource();
        if (ChooseSound) {
            PlayMusic(choose);
        }
    }
    void PlayUnChoose() {
        ResetAudioSource();
        if (UnChooseSound) {
            PlayMusic(unChoose);
        }
    }
    void PlayCantMove() {
        ResetAudioSource();
        if (CantMoveSound) {
            PlayMusic(cantMove);
        }
    }
    void PlayMove() {
        ResetAudioSource();
        if (MoveSound) {
            PlayMusic(move);
        }
    }
    void PlayCapture() {
        ResetAudioSource();
        if (CaptureSound) {
            PlayMusic(capture);
        }
    }
    void PlayCheck() {
        ResetAudioSource();
        if (CheckSound) {
            PlayMusic(check);
        }
    }
    void PlayCheckmate() {
        ResetAudioSource();
        if (CheckmateSound) {
            PlayMusic(checkmate);
        }
    }
    void PlayPromote() {
        ResetAudioSource();
        if (PromoteSound) {
            PlayMusic(promote);
        }
    }
    public void LetsDropTheBeat() {
        if (audio.clip != null && audio.clip.name == unity.name) {
            if (audio.isPlaying == false) {
                MeshRendererManager.SetMusicFactor();
                audio.UnPause();
                UIManager.UIM.MapHolderRotate = true;
                return;
            } else {
                audio.Pause();
                UIManager.UIM.MapHolderRotate = false;
                return;
            }
        } else {
            MeshRendererManager.SetMusicFactor();
            ResetAudioSource();
            PlayMusic(unity);
            UIManager.UIM.MapHolderRotate = true;
        }
    }
    private void ResetAudioSource() {
        audio.clip = null;
    }
    private void WeakAudioVolume() {
        StartCoroutine(WeakingAudioVolume());
    }
    IEnumerator WeakingAudioVolume() {
        float volume = audio.volume;
        float t = 0;
        float timeInterval = 1f;
        while (t < timeInterval) {
            t += Time.deltaTime;
            audio.volume = Mathf.Lerp(audio.volume, 0, t / timeInterval);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        audio.Stop();
        audio.volume = volume;
        yield return 0;
    }
    void SetVolume(float volume) {
        audio.volume = volume;
    }
}

