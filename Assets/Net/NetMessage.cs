using Unity.Collections;
using UnityEngine;

public class NetMessage  {
    public OpCode code { set; get; }

    public virtual void Serialize(ref DataStreamWriter writer) { 
        
    }

    public virtual void Deserialize(DataStreamReader reader) { 
        
    }

    public virtual void ReceivedOnClient() { 
        
    }

    public virtual void ReceivedOnServer(BaseServer server) { 
        
    }
}