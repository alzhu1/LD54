using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus : MonoBehaviour {
    public static EventBus instance = null;

    public event Action<int> OnLevelStart = delegate {};
    public event Action OnLevelFail = delegate {};
    public event Action OnLevelComplete = delegate {};

    public event Action<int> OnJump = delegate {};

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }
    }

    void Start() {
        
    }

    // private int t = 0;
    void Update() {
        // if (Input.GetKeyDown(KeyCode.Z)) {
        //     t = (t + 1) % 5;
        //     TriggerOnLevelStart(t);
        // }
    }

    public void TriggerOnLevelStart(int levelIndex) {
        OnLevelStart?.Invoke(levelIndex);
    }

    public void TriggerOnLevelFail() {
        Debug.Log("FAILED");
        OnLevelFail?.Invoke();
    }

    public void TriggerOnLevelComplete() {
        Debug.Log("COMPLETED");
        OnLevelComplete?.Invoke();
    }

    public void TriggerOnJump(int jumpsLeft) {
        OnJump?.Invoke(jumpsLeft);
    }
}
