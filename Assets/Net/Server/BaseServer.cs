using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;

public class BaseServer : MonoBehaviour {
    public NetworkDriver driver; // Driver to speak with connections
    protected NativeList<NetworkConnection> connections; // Connected devices
    [SerializeField] private string ipAddress = "127.0.0.1";
    [SerializeField] private ushort port = 8000;

#if UNITY_EDITOR
    private void Start() { Init(); }
    private void Update() { UpdateServer(); }
    private void OnDestroy() { ShutDown(); }
#endif

    // Initialize server
    public virtual void Init() {
        // Initialize the driver
        driver = NetworkDriver.Create();
        NetworkEndpoint endpoint = NetworkEndpoint.Parse(ipAddress, port);

        if (driver.Bind(endpoint) != 0)
            Debug.Log("Error! Driver couldn't bind to port " + endpoint.Port);
        else
            driver.Listen();

        // Initialize the connection list
        connections = new NativeList<NetworkConnection>(1, Allocator.Persistent);
    }

    // Keep track of everything
    public virtual void UpdateServer() {
        driver.ScheduleUpdate().Complete();
        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }

    // Get rid of any connections that are 'empty'
    private void CleanupConnections() {
        for (int i = 0; i < connections.Length; i++) {
            if (!connections[i].IsCreated) {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }

    // Add connections if possible
    private void AcceptNewConnections() {
        NetworkConnection connection;
        while ((connection = driver.Accept()) != default(NetworkConnection)) {
            connections.Add(connection);
            Debug.Log("Accepted a connection");
        }
    }

    // Check for messages
    protected virtual void UpdateMessagePump() {
        DataStreamReader stream;
        for (int i = 0; i < connections.Length; i++) {
            NetworkEvent.Type cmd;
            // If there are messages
            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty) {
                // If it's data sent through the driver
                if (cmd == NetworkEvent.Type.Data) {
                    OnData(stream);
                // If it's a disconnection
                } else if (cmd == NetworkEvent.Type.Disconnect) {
                    Debug.Log("Client disconnected from the server");
                    connections[i] = default(NetworkConnection);
                }
            }
        }
    }

    // Dispose of driver and connections
    public virtual void ShutDown() {
        driver.Dispose();
        connections.Dispose();
    }

    // Handle received data
    public virtual void OnData(DataStreamReader stream) {
        NetMessage msg = null;
        var opCode = (OpCode)stream.ReadByte();
        switch (opCode) { 
            case OpCode.TEST: msg = new NetTest(stream); break;
            default: Debug.Log("Message received had no Operation Code"); break;
        }

        msg.ReceivedOnServer(this);
    }

    // Send data to a specific connection
    public virtual void SendToClient(NetworkConnection connection, NetMessage msg) {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    // Send data to all connections
    public virtual void Broadcast(NetMessage msg) {
        for (int i = 0; i < connections.Length; i++) {
            if (connections[i].IsCreated) { 
                SendToClient(connections[i], msg);
            } 
        }
    }
}
