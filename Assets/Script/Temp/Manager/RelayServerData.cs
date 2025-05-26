using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;

// RelayServerData struct to handle allocation data properly
public struct RelayServerData
{
    public string ServerIp;
    public ushort ServerPort;
    public byte[] AllocationIdBytes;
    public byte[] Key;
    public byte[] ConnectionData;
    public byte[] HostConnectionData;
    public string Protocol;

    public RelayServerData(Allocation allocation, string protocol = "udp")
    {
        ServerIp = allocation.RelayServer.IpV4;
        ServerPort = (ushort)allocation.RelayServer.Port;
        AllocationIdBytes = allocation.AllocationIdBytes;
        Key = allocation.Key;
        ConnectionData = allocation.ConnectionData;
        HostConnectionData = allocation.ConnectionData;
        Protocol = protocol;
    }

    public void ApplyTo(UnityTransport transport)
    {
        transport.SetRelayServerData(
            ServerIp,
            ServerPort,
            AllocationIdBytes,
            Key,
            ConnectionData,
            HostConnectionData,
            Protocol == "udp"
        );
    }
}

