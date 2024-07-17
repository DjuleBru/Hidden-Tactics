using Modern2D;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Unit : NetworkBehaviour, ITargetable {

    public event EventHandler OnUnitUpgraded;
    public event EventHandler OnUnitSpawned;
    public event EventHandler OnUnitPlaced;
    public event EventHandler OnUnitDied;
    public event EventHandler OnUnitReset;
    public event EventHandler OnUnitFell;
    public event EventHandler OnUnitSold;
    public event EventHandler<OnUnitDazedEventArgs> OnUnitDazed;
    public event EventHandler OnUnitSetAsAdditionalUnit;
    public event EventHandler OnAdditionalUnitActivated;

    [SerializeField] private Transform projectileTarget;
    [SerializeField] private UnitVisual unitVisual;
    [SerializeField] private UnitUI unitUI;
    public class OnUnitDazedEventArgs : EventArgs {
        public float dazedTime;
    }

    [SerializeField] protected UnitSO unitSO;
    protected Troop parentTroop;
    protected Building parentBuilding;

    protected GridPosition initialUnitGridPosition;
    protected GridPosition currentGridPosition;
    protected Vector3 unitPositionInTroop;

    protected Rigidbody2D rb;
    protected Collider2D collider2d;
    protected bool unitIsDead;
    protected bool unitIsPlaced;
    protected bool isAdditionalUnit;
    protected bool unitIsBought;
    protected bool unitIsOnlyVisual;

    protected virtual void Awake() {
        collider2d = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        collider2d.enabled = false;

        rb.mass = unitSO.mass;
    }

    protected virtual void Start() {
        if(!unitIsOnlyVisual)
        {
            BattleManager.Instance.OnStateChanged += BattleManager_OnStateChanged;
        }
    }

    protected void Update() {
        HandlePositionOnGrid();
        if(IsServer && BattleManager.Instance.IsBattlePhase()) {
            HandlePositionSyncServerRpc(transform.position);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    protected void HandlePositionSyncServerRpc(Vector3 position) {
        HandlePositionSyncClientRpc(position);
    }

    [ClientRpc]
    protected void HandlePositionSyncClientRpc(Vector3 position) {
        if (!IsServer) {
            // Mirror x position 
            transform.position = new Vector3(position.x + (BattleGrid.Instance.GetBattlefieldMiddlePoint() - position.x) * 2, position.y, 0);
        }
    }

    public void HandlePositionOnGrid() {
        GridPosition newGridPosition = BattleGrid.Instance.GetGridPosition(transform.position);

        // Grid position is not a valid position
        if (!BattleGrid.Instance.IsValidGridPosition(newGridPosition)) return;

        // Unit was not set at a grid position yet
        if (currentGridPosition == null) {

            currentGridPosition = BattleGrid.Instance.GetGridPosition(transform.position);
            BattleGrid.Instance.AddUnitAtGridPosition(currentGridPosition, this);
        }

        // Unit changed grid position
        if (newGridPosition != currentGridPosition) {
            BattleGrid.Instance.UnitMovedGridPosition(this, currentGridPosition, newGridPosition);
            currentGridPosition = newGridPosition;
        }
    }

    protected void BattleManager_OnStateChanged(object sender, EventArgs e) {
        if(BattleManager.Instance.IsBattlePhaseEnding()) {
            ResetUnit();
        }
    }
    protected virtual void ParentTroop_OnTroopPlaced(object sender, System.EventArgs e) {

        if (!isAdditionalUnit) {
            unitIsPlaced = true;
            collider2d.enabled = true;
        }

        if(unitSO.isGarrisonedUnit) {
            collider2d.enabled = false;
            SetParentBuilding();
        }

        OnUnitPlaced?.Invoke(this, EventArgs.Empty);
    }

    public void SetParentBuilding() {
        GridPosition parentTroopCenterPointGridPosition = BattleGrid.Instance.GetGridPosition(parentTroop.GetTroopCenterPoint());
        Building parentBuilding = BattleGrid.Instance.GetBuildingListAtGridPosition(parentTroopCenterPointGridPosition)[0];
        this.parentBuilding = parentBuilding;
        parentBuilding.OnBuildingDestroyed += ParentBuilding_OnBuildingDestroyed;
    }

    private void ParentBuilding_OnBuildingDestroyed(object sender, EventArgs e) {
        HiddenTacticsMultiplayer.Instance.DestroyIPlaceable(parentTroop.GetComponent<NetworkObject>());
    }

    public virtual void ResetUnit() {
        transform.localPosition = unitPositionInTroop;
        unitIsDead = false;

        if (!unitSO.isGarrisonedUnit & unitIsBought) {
            collider2d.enabled = true;
        }
        OnUnitReset?.Invoke(this, EventArgs.Empty);
    }

    public virtual void UpgradeUnit() {
        OnUnitUpgraded?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnUnit() {
        OnUnitSpawned?.Invoke(this, EventArgs.Empty);
    }

    public void TakeKnockBack(Vector2 force) {
        rb.AddForce(force);
    }

    public void TakeDazed(float dazedTime) {
        OnUnitDazed?.Invoke(this, new OnUnitDazedEventArgs {
            dazedTime = dazedTime
        });
    }

    public void Die() {
        OnUnitDied?.Invoke(this, EventArgs.Empty);
        unitIsDead = true;
        collider2d.enabled = false;

        RemoveUnitFromBattlePhaseUnitList();
    }

    public void Fall() {
        OnUnitFell?.Invoke(this, EventArgs.Empty);
        unitIsDead = true;
        collider2d.enabled = false;
        RemoveUnitFromBattlePhaseUnitList();
    }

    public void SellUnit() {
        OnUnitSold?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveUnitFromBattlePhaseUnitList() {
        if (IsServer) {
            StartCoroutine(RemoveUnitFromBattlePhaseUnitListCoroutine());
        }
    }

    private IEnumerator RemoveUnitFromBattlePhaseUnitListCoroutine() {
        yield return new WaitForSeconds(2f);
        BattleManager.Instance.RemoveUnitFromUnitsStillInBattleList(this);
    }

    #region GET PARAMETERS

    public Vector3 GetUnitPositionInTroop() {
        return unitPositionInTroop;
    }
    public bool GetIsDead() {
        return unitIsDead;
    }

    public bool GetUnitIsPlaced() {
        return unitIsPlaced;
    }

    public bool GetUnitIsBought() {
        return unitIsBought;
    }

    public bool GetUnitIsAdditionalUnit() {
        return isAdditionalUnit;
    }

    public UnitSO GetUnitSO() {
        return unitSO;
    }

    public UnitUI GetUnitUI() {
        return unitUI;
    }

    public Troop GetParentTroop() {
        return parentTroop;
    }

    public ITargetable.TargetType GetTargetType() {
        return unitSO.unitTargetType;
    }

    public Transform GetProjectileTarget() {
        return projectileTarget;
    }

    public GridPosition GetCurrentGridPosition() {
        return currentGridPosition;
    }

    public GridPosition GetInitialUnitGridPosition() {
        return initialUnitGridPosition;
    }

    public bool IsOwnedByPlayer() {
        return parentTroop.IsOwnedByPlayer();
    }

    public IDamageable GetIDamageable() {
        return GetComponent<UnitHP>();
    }

    public UnitVisual GetUnitVisual() {
        return unitVisual;
    }

    #endregion

    #region SET PARAMETERS

    public void SetParentTroop(Troop parentTroop) {
        this.parentTroop = parentTroop;
        parentTroop.OnTroopPlaced += ParentTroop_OnTroopPlaced;

        parentTroop.AddUnitToUnitInTroopList(this);
    }

    public void SetPosition(Vector3 positionInTroop) {

        if(parentTroop.IsOwnedByPlayer()) {

            unitPositionInTroop = positionInTroop;
        } else {
            //Mirror x position in troop
            float mirroredPositionX = positionInTroop.x + (parentTroop.GetTroopCenterPoint().x - positionInTroop.x) * 2;
            unitPositionInTroop = new Vector3(mirroredPositionX, positionInTroop.y, 0);
        }

        transform.position = unitPositionInTroop;
    }

    public void SetInitialGridPosition(GridPosition gridPosition) {
        initialUnitGridPosition = gridPosition;
    }

    public void SetUnitAsAdditionalUnit() {
        isAdditionalUnit = true;
        parentTroop.AddUnitToAdditionalUnitsInTroopList(this);

        OnUnitSetAsAdditionalUnit?.Invoke(this, EventArgs.Empty);
        
        unitVisual.gameObject.SetActive(false);
        GetComponent<Collider2D>().enabled = false;
        GetComponent<UnitMovement>().enabled = false;
        GetComponent<UnitAI>().enabled = false;
        GetComponent<UnitAttack>().enabled = false;
        GetComponent<UnitTargetingSystem>().enabled = false;
        GetComponent<UnitHP>().enabled = false;
    }

    #endregion

    public void InvokeOnUnitPlaced() {
        OnUnitPlaced?.Invoke(this, EventArgs.Empty);
    }

    protected void InvokeOnUnitReset() {
        OnUnitReset?.Invoke(this, EventArgs.Empty);
    }

    public void ActivateAdditionalUnit() {

        if(IsServer) {
            if (!BattleManager.Instance.GetUnitsInBattlefieldList().Contains(this)) {
                BattleManager.Instance.AddUnitToUnitListInBattlefield(this);
            }
        }

        ActivateAdditionalUnitServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ActivateAdditionalUnitServerRpc() {
        ActivateAdditionalUnitClientRpc();
    }

    [ClientRpc]
    private void ActivateAdditionalUnitClientRpc() {
        unitIsBought = true;
        OnAdditionalUnitActivated?.Invoke(this, EventArgs.Empty);

        unitVisual.gameObject.SetActive(true);
        GetComponent<Collider2D>().enabled = true;
        GetComponent<UnitMovement>().enabled = true;
        GetComponent<UnitAI>().enabled = true;
        GetComponent<UnitAttack>().enabled = true;
        GetComponent<UnitTargetingSystem>().enabled = true;
        GetComponent<UnitHP>().enabled = true;
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    public void DebugModeStartFunction() {
        OnUnitPlaced?.Invoke(this, EventArgs.Empty);
        unitIsBought = true;
        collider2d.enabled = true;
    }

    public void SetUnitAsVisual()
    {
        unitIsOnlyVisual = true;
        GetComponent<UnitMovement>().enabled = false;
        GetComponent<UnitTargetingSystem>().enabled = false;
        GetComponent<UnitAttack>().enabled = false;
        GetComponent<NetworkObject>().enabled = false;
        GetComponent<UnitHP>().enabled = false;
        GetComponent<SimpleSmoothModifier>().enabled = false;
        GetComponentInChildren<UnitUI>().enabled = false;
        GetComponentInChildren<UnitAI>().enabled = false;

        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if(child.GetComponent<StylizedShadowCaster2D>() != null)
            {
                child.GetComponent<StylizedShadowCaster2D>().enabled = false;
            }
        }
    }

    public bool GetUnitIsOnlyVisual()
    {
        return unitIsOnlyVisual;
    }

    public override void OnDestroy() {
        base.OnDestroy();

        if (unitIsOnlyVisual) return;

        BattleManager.Instance.RemoveUnitFromUnitListInBattlefield(this);
        BattleManager.Instance.OnStateChanged -= BattleManager_OnStateChanged;
    }

    public bool GetIsServer() {
        return IsServer;
    }

}
