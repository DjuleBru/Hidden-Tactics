using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable {

    public ulong clientId;
    public int truePlayerNumber;

    public FixedString64Bytes playerId;

    public int playerGold;
    public int playerVillagesRemaining;
    public int playerRevenue;

    public bool Equals(PlayerData other) {
        return clientId == other.clientId && 
            playerId == other.playerId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref truePlayerNumber);
        serializer.SerializeValue(ref playerId);

        serializer.SerializeValue(ref playerGold);
        serializer.SerializeValue(ref playerVillagesRemaining);
        serializer.SerializeValue(ref playerRevenue);

    }
}
