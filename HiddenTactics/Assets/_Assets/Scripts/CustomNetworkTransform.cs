using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class CustomNetworkTransform : NetworkTransform
{
    protected override void OnNetworkTransformStateUpdated(ref NetworkTransform.NetworkTransformState oldState, ref NetworkTransform.NetworkTransformState NewState) {

    }
}
