using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckSlotVisualSpawnPosition : MonoBehaviour
{
    [SerializeField] private int spawnPointNumber;
    [SerializeField] private Transform canvas;

    private void Awake() {
        canvas.gameObject.SetActive(false);
    }

    public int GetSpawnPointNumber() {
        return spawnPointNumber;
    }
}
