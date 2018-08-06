﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.GamePackets
{
    public class S2C_FloatingText2 : GamePacket, IUnusedPacket // 0x128
    {
        public override GamePacketID ID => GamePacketID.S2C_FloatingText2;
        //FIXME: 4.18+
        public static S2C_FloatingText2 CreateBody(PacketReader reader, NetID senderNetID)
        {
            var result = new S2C_FloatingText2();
            result.SenderNetID = senderNetID;
        
            return result;
        }
        public override void WriteBody(PacketWriter writer)
        {
        }
    }
}