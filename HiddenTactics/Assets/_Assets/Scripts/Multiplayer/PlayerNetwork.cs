using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField] Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;

    // Network Variables can only accept Ints
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1);

    private NetworkVariable<MyCustomData> randomNumber2 = new NetworkVariable<MyCustomData>(
        new MyCustomData {
            _int = 50,
            _bool = true,
        });

    // Custom class for network Variables
    public struct MyCustomData : INetworkSerializable {
        public int _int;
        public bool _bool;
        public FixedString32Bytes message;

        // We MUST serialize all fields from the custom class in here
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }


    // Initialise listeners in OnNetworkSpawn, never in Start/Awake
    public override void OnNetworkSpawn() {
        randomNumber.OnValueChanged += (int previousValue, int newValue) => {
            Debug.Log(OwnerClientId + " ; " + randomNumber.Value);
        };

        randomNumber2.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) => {
            Debug.Log(OwnerClientId + " ; " + newValue._int + "; " + newValue._bool + " ; " + newValue.message);
        };
    }

    void Update() {
        if (!IsServer) return;

        if((Input.GetKeyDown(KeyCode.T))) {
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
            //TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } } );

            //randomNumber.Value = Random.Range(0, 100);
        }

        if ((Input.GetKeyDown(KeyCode.F))) {

            Destroy(spawnedObjectTransform.gameObject);

            //randomNumber2.Value = new MyCustomData {
            //    _int = 10,
            //    _bool = false,
            //    message = "Her we go!"
            //};
        }

        Vector3 moveDir = Vector3.zero;
        if(Input.GetKey(KeyCode.Z)) {
            moveDir.y = 1;
        }
        if (Input.GetKey(KeyCode.S)) {
            moveDir.y = -1;
        }
        if (Input.GetKey(KeyCode.Q)) {
            moveDir.x = -1;
        }
        if (Input.GetKey(KeyCode.D)) {
            moveDir.x = 1;
        }

        float moveSpeed = 3f;

        transform.position += moveSpeed * moveDir * Time.deltaTime;
    }


    // SERVER RPC : Send data from a client to the server
    // MUST end with ServerRpc and
    // MUST implement [ServerRpc] and
    // MUST be in a NetworkBehaviour class and
    // MUST be attached to a gameObject with a Network object

    // Note : works with strings !
    [ServerRpc]
    private void TestServerRpc(string message) {
        Debug.Log("TestServerRpc " + OwnerClientId + "; " + message);
    }

    // CLIENT RPC : Send data from the server to clients
    // With ClientRpcParams, we can specify a list of target clients that receive the function
    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams) {
        Debug.Log("TestClientRpc");
    }
}
