﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.Game
{
    public class S2C_StartSpellTargeter : GamePacket // 0x129
    {
        public override GamePacketID ID => GamePacketID.S2C_StartSpellTargeter;
        public byte Slot { get; set; }
        // Amount of time to check for target.
        public float TargetTime { get; set; }

        protected override void ReadBody(ByteReader reader)
        {
            Slot = (byte)reader.ReadUInt32();
            TargetTime = reader.ReadFloat();
        }
        protected override void WriteBody(ByteWriter writer)
        {
            writer.WriteUInt32((uint)Slot);
            writer.WriteFloat(TargetTime);
        }
    }
}
