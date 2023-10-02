using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [SerializeField] private Text jumpsLeftText;
    [SerializeField] private GameObject loseModal;

    void Start() {
        EventBus.instance.OnLevelStart += ReceiveLevelStartEvent;
        EventBus.instance.OnLevelFail += ReceiveLevelFailEvent;
        EventBus.instance.OnJump += ReceiveJumpEvent;
    }

    void OnDestroy() {
        EventBus.instance.OnLevelStart -= ReceiveLevelStartEvent;
        EventBus.instance.OnLevelFail -= ReceiveLevelFailEvent;
        EventBus.instance.OnJump -= ReceiveJumpEvent;
    }

    void ReceiveLevelStartEvent(int levelIndex) {
        loseModal.SetActive(false);
    }

    void ReceiveLevelFailEvent() {
        loseModal.SetActive(true);
    }

    void ReceiveJumpEvent(int numJumpsLeft) {
        Debug.Log("Update jump UI");
        jumpsLeftText.text = $"Jumps Left: {numJumpsLeft}";
    }
}
