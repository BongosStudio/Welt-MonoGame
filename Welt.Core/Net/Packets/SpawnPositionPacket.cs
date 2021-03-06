﻿using Lidgren.Network;
using System;
using Welt.API.Net;

namespace Welt.Core.Net.Packets
{
    /// <summary>
    /// Sent by the server to specify the coordinates of the spawn point. This only affects what the
    /// compass item points to.
    /// </summary>
    public struct SpawnPositionPacket : IPacket
    {
        public byte Id => 0x06;

        public SpawnPositionPacket(int x, int z)
        {
            X = x;
            Z = z;
        }

        public int X, Z;

        public void ReadPacket(NetIncomingMessage stream)
        {
            X = stream.ReadInt32();
            Z = stream.ReadInt32();
        }

        public void WritePacket(NetOutgoingMessage stream)
        {
            stream.Write(X);
            stream.Write(Z);
        }
    }
}