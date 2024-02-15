using Unity.Collections;
using UnityEngine;

public class NetTest : NetMessage {
    public FixedString128Bytes data { set; get; }

    // Constructors
    public NetTest(FixedString128Bytes data) {
        code = OpCode.TEST;
        this.data = data;
    }

    public NetTest(DataStreamReader reader) { 
        code = OpCode.TEST;
        Deserialize(reader);
    }

    public NetTest() {
        code = OpCode.TEST;
    }
    
    // Serialize
    public override void Serialize(ref DataStreamWriter writer) {
        writer.WriteByte((byte)code);
        writer.WriteFixedString128(data);
    }

    public override void Deserialize(DataStreamReader reader) { 
        data = reader.ReadFixedString128();
    }

    public override void ReceivedOnClient() {
        Debug.Log("CLIENT::" + data);
    }

    public override void ReceivedOnServer(BaseServer server) {
        Debug.Log("SERVER::" + data);
        server.Broadcast(this);
    }
}
