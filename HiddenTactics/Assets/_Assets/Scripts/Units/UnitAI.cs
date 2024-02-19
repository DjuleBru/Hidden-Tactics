using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAI : MonoBehaviour
{
    private Unit unit;
    private bool unitActive;

    private Vector3 moveDir2D;

    public enum State {
        idle,
        walking,
        attacking,
    }
    private State state;
    public event EventHandler OnStateChanged;

    private void Awake() {
        unit = GetComponent<Unit>();
        state = State.idle;
    }

    private void Start() {
        BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
    }

    private void Update() {
        if (!unitActive) return;

        switch(state) {
            case State.idle:
                break; 
            case State.walking:
                Move();
                break;
            case State.attacking:
                break;

        }

    }

    private void Move() {
        moveDir2D = new Vector2(1, 0);
        Vector3 moveDir3D = new Vector3(moveDir2D.x, moveDir2D.y, 0);

        transform.position += moveDir3D * unit.GetUnitSO().unitMoveSpeed * Time.deltaTime;
    }

    private void BattleManager_OnStateChanged(object sender, System.EventArgs e) {
        unitActive = BattleManager.Instance.IsBattlePhase();
        if(unitActive) {
            state = State.walking;
        } else {
            state = State.idle;
        }

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMoveDir2D() {
        return moveDir2D;
    }

    public bool isWalking() {
        return state == State.walking;
    }

    public bool isIdle() {
        return state == State.idle;
    }

}
