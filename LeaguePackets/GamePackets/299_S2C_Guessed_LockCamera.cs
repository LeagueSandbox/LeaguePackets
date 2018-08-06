﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.GamePackets
{
    public class S2C_Guessed_LockCamera : GamePacket, IUnusedPacket // 0x12B
    {
        public override GamePacketID ID => GamePacketID.S2C_Guessed_LockCamera;
        //FIXME: 4.18+
        public static S2C_Guessed_LockCamera CreateBody(PacketReader reader, NetID senderNetID)
        {
            var result = new S2C_Guessed_LockCamera();
            result.SenderNetID = senderNetID;
        
            return result;
        }
        public override void WriteBody(PacketWriter writer)
        {
        }
    }
}