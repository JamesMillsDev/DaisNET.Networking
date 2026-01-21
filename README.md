# DaisNET.Networking

A lightweight TCP socket networking library for C# with built-in message framing and packet serialization.

## Contents

- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
    - [Server Setup](#server-setup)
    - [Client Setup](#client-setup)
    - [Creating Custom Packets](#creating-custom-packets)
    - [Sending Packets](#sending-packets)
- [Architecture](#architecture)
    - [Message Framing](#message-framing)
    - [Packet System](#packet-system)
    - [Supported Data Types](#supported-data-types)
- [Advanced Usage](#advanced-usage)
    - [Custom Serializable Types](#custom-serializable-types)
    - [Server Events](#server-events)
    - [Connection Management](#connection-management)
- [Configuration](#configuration)
- [Best Practices](#best-practices)
- [Thread Safety](#thread-safety)
- [Requirements](#requirements)
- [Examples](#examples)
- [Contributing](#contributing)
- [License](#license)
- [Author](#author)
- [Acknowledgments](#acknowledgments)

## Features

- Length-prefixed message framing over TCP to handle packet boundaries
- Automatic packet serialization/deserialization
- Client-server architecture with authoritative server support
- Asynchronous networking with timeout protection
- Type-safe packet system with custom serialization support
- Thread-safe connection management
- Support for broadcasting packets to multiple clients

## Installation

Add DaisNET.Networking to your project via Git Submodules:

```bash
git submodule add https://github.com/JamesMillsDev/DaisNET.Networking.git
```

## Quick Start

### Server Setup

```csharp
using DaisNET.Networking;

// Create and start a server
Network.CreateServer();
Network.Instance.Open();

// Register packet types
Network.Instance.RegisterPacket("player_move", typeof(PlayerMovePacket));

// Run the network loop (typically in a background task)
await Network.RunNetworkLoop(args, yourApplication);
```

### Client Setup

```csharp
using DaisNET.Networking;

// Create and connect to a server
Network.CreateClient("localhost");
Network.Instance.Open();

// Register the same packet types as the server
Network.Instance.RegisterPacket("player_move", typeof(PlayerMovePacket));

// Run the network loop
await Network.RunNetworkLoop(args, yourApplication);
```

### Creating Custom Packets

```csharp
using DaisNET.Networking.Packets;

public class PlayerMovePacket : Packet
{
    private float x;
    private float y;

    public PlayerMovePacket() : base("player_move") { }

    public override void Serialize(PacketWriter writer)
    {
        writer.Write(x);
        writer.Write(y);
    }

    public override void Deserialize(PacketReader reader)
    {
        x = reader.ReadFloat();
        y = reader.ReadFloat();
    }

    public override async Task Process()
    {
        // Handle the packet when received
        Console.WriteLine($"Player moved to ({x}, {y})");
    }
}
```

### Sending Packets

```csharp
// Create and send a packet
PlayerMovePacket packet = new PlayerMovePacket();
Network.Instance.SendPacket(packet);

// Server: Broadcast to all clients
if (Network.Instance is NetworkServer server)
{
    server.BroadcastPacket(packet);
}
```

## Architecture

### Message Framing

DaisNET.Networking solves TCP's stream-based nature by using length-prefixed framing:

```
[4-byte length][packet ID length][packet ID][payload data]
```

This ensures packets are read completely and correctly, even when:
- Multiple packets arrive in a single TCP segment
- A single packet is split across multiple TCP segments
- Network latency causes irregular data arrival

### Packet System

All packets inherit from the `Packet` base class and must implement:
- `Serialize()` - Write packet data to a PacketWriter
- `Deserialize()` - Read packet data from a PacketReader
- `Process()` - Handle the packet when received

Packets are automatically routed and instantiated based on their registered ID.

### Supported Data Types

PacketReader and PacketWriter support:
- Primitives: `bool`, `byte`, `char`, `short`, `int`, `long`, `float`, `double`
- Unsigned: `ushort`, `uint`, `ulong`
- Strings (UTF-8 encoded with length prefix)
- Custom types implementing `IPacketSerializable`

## Advanced Usage

### Custom Serializable Types

```csharp
public class PlayerData : IPacketSerializable
{
    public string Name;
    public int Level;

    public byte[] Serialize()
    {
        using var writer = new PacketWriter("");
        writer.Write(Name);
        writer.Write(Level);
        return writer.GetBytes();
    }

    public void Deserialize(byte[] data)
    {
        var reader = new PacketReader(data);
        Name = reader.ReadString();
        Level = reader.ReadInt32();
    }

    public int GetSize()
    {
        return sizeof(int) + Encoding.UTF8.GetByteCount(Name) + sizeof(int);
    }
}
```

### Server Events

```csharp
if (Network.Instance is NetworkServer server)
{
    server.OnClientConnected += (clientId) =>
    {
        Console.WriteLine($"Client {clientId} connected");
    };
}
```

### Connection Management

```csharp
// Check connection status
if (Network.Instance is NetworkClient client)
{
    if (client.Connected)
    {
        // Send data
    }
}

// Gracefully close connection
Network.Instance.Close();
```

## Configuration

### Poll Rate

Adjust how frequently the network polls for incoming data:

```csharp
// Default is 20ms
Network.CreateServer("localhost", port: 25565, pollRate: 16); // ~60Hz
```

### Connection Timeout

Each receive operation has a 1-second timeout by default. This prevents indefinite blocking when clients disconnect unexpectedly.

## Best Practices

1. **Always register packets on both client and server** - Packet types must match between endpoints
2. **Keep packets small** - Network bandwidth is limited; send only necessary data
3. **Validate data on the server** - Never trust client input; the server has authority
4. **Handle disconnections gracefully** - Wrap network operations in try-catch blocks
5. **Use the polling system** - Don't block the network thread with heavy processing in `Process()`

## Thread Safety

- Connection lists are thread-safe (using locks)
- Packet sending is thread-safe
- Each client connection is handled independently

## Requirements

- .NET 9.0 or higher
- No external dependencies

## Examples

See the '[PongNetworked](https://github.com/JamesMillsDev/PongNetworked)' directory for complete working examples including:
- Simple echo server/client
- Multiplayer game state synchronization
- Chat application

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Author

James Mills - 2026 - [@JamesMillsDev](https://github.com/JamesMillsDev)

## Acknowledgments

Built as a learning project for understanding TCP networking, async/await patterns, and game networking fundamentals.