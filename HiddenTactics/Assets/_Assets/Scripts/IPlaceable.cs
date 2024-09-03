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
    void SetIPlaceableID(int id);
    void SetIPlaceablePlaced();
    void DeActivateOpponentIPlaceable();
    void ReplaceLocalIPleaceable();
    void SetIPlaceableGridPosition(GridPosition iPlaceableGridPosition);

    GridPosition GetIPlaceableGridPosition();

    void DestroySelf();
    bool GetIsSpawnedOnServer();
    bool GetSelected();
    bool IsOwnedByPlayer();
    int GetIPlaceableID();
}
