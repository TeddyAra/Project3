using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class BaseClient : MonoBehaviour {
    public NetworkDriver driver; // Driver to speak with connections
    protected NetworkConnection connection; // Connected device (Server)
    [SerializeField] private ushort port = 8000;

#if UNITY_EDITOR
    private void Start() { Init(); }
    private void Update() { UpdateServer(); }
    private void OnDestroy() { ShutDown(); }
#endif

    // Initialize client
    public virtual void Init() {
        // Initialize the driver
        driver = NetworkDriver.Create();
        connection = default(NetworkConnection);

        NetworkEndpoint endpoint = NetworkEndpoint.LoopbackIpv4;
        endpoint.Port = port;
        connection = driver.Connect(endpoint);
    }

    // Keep track of everything
    public virtual void UpdateServer() {
        driver.ScheduleUpdate().Complete();
        UpdateMessagePump();
        CheckAlive();
    }

    // Check is client is still connected to the server
    private void CheckAlive() {
        if (!connection.IsCreated) {
            Debug.Log("Something went wrong, lost connection to server");
        }
    }

    // Check for messages
    protected virtual void UpdateMessagePump() {
        DataStreamReader stream;

        NetworkEvent.Type cmd;
        // If there are messages
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty) {
            // If it's a connection
            if (cmd == NetworkEvent.Type.Connect) {
                Debug.Log("We are now connected to the server");
            // If it's data
            } else if (cmd == NetworkEvent.Type.Data) {
                OnData(stream);
            // If it's a disconnection
            } else if (cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Client got disconnected from the server");
                connection = default(NetworkConnection);
            }
        }
    }

    // Dispose of driver
    public virtual void ShutDown() {
        driver.Dispose();
    }

    // Send message to server
    public virtual void SendToServer(NetMessage msg) {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    // Handle received data
    public virtual void OnData(DataStreamReader stream) {
        NetMessage msg = null;
        var opCode = (OpCode)stream.ReadByte();
        switch (opCode) { 
            case OpCode.TEST: msg = new NetTest(stream); break;
            default: Debug.Log("Message received had no Operation Code"); break;
        }

        msg.ReceivedOnClient();
    }
}
