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
    void SetIPlaceableGridPosition(GridPosition iPlaceableGridPosition);

    void SetIPlaceableBattlefieldParent(GridPosition iPlaceableGridPosition);

    GridPosition GetIPlaceableGridPosition();

    void DestroySelf();

    bool IsOwnedByPlayer();

}
