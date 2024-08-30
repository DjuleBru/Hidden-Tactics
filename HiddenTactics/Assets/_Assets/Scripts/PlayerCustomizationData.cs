using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerCustomizationData : IEquatable<PlayerCustomizationData>, INetworkSerializable {

    public ulong clientId;

    public int factionID;
    public int iconSpriteId;
    public int battlefieldBaseSOId;
    public int gridVisualSOId;

    public int villageSpriteNumber;
    public int villageSprite0Id;
    public int villageSprite1Id;
    public int villageSprite2Id;
    public int villageSprite3Id;
    public int villageSprite4Id;
    public int villageSprite5Id;

    public FixedString64Bytes playerName;
    public FixedString64Bytes playerId;

    public bool Equals(PlayerCustomizationData other) {
        return clientId == other.clientId &&
            playerName == other.playerName &&
            playerId == other.playerId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerId);

        serializer.SerializeValue(ref villageSpriteNumber);
        serializer.SerializeValue(ref villageSprite0Id);
        serializer.SerializeValue(ref villageSprite1Id);
        serializer.SerializeValue(ref villageSprite2Id);
        serializer.SerializeValue(ref villageSprite3Id);
        serializer.SerializeValue(ref villageSprite4Id);
        serializer.SerializeValue(ref villageSprite5Id);

        serializer.SerializeValue(ref factionID);
        serializer.SerializeValue(ref iconSpriteId);
        serializer.SerializeValue(ref battlefieldBaseSOId);
        serializer.SerializeValue(ref gridVisualSOId);
    }

}
