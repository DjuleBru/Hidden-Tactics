using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseCursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D cursorTexture;
    private CursorMode cursorMode = CursorMode.Auto;
    private Vector2 hotSpot = Vector2.zero;
    void Start() {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

}
