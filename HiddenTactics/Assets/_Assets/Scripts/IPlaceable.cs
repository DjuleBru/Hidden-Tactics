using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlaceable {


    void HandlePositioningOnGrid();
    void HandleIPlaceablePositionDuringPlacement();

    void HandleIPlaceablePosition();

    void PlaceIPlaceable();

    void SetIPlaceableOwnerClientId(ulong clientId);
    void SetIPlaceableBattlefieldOwner();
    void SetAsPooledIPlaceable();
    void SetPlacingIPlaceable();
    void DeActivateOpponentIPlaceable();
    void SetIPlaceableGridPosition(GridPosition iPlaceableGridPosition);

    GridPosition GetIPlaceableGridPosition();

    void DestroySelf();

    bool GetSelected();
    bool IsOwnedByPlayer();

}
