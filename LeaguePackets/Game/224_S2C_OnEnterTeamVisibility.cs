﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.Game
{
    public class S2C_OnEnterTeamVisibility : GamePacket // 0xE0
    {
        public override GamePacketID ID => GamePacketID.S2C_OnEnterTeamVisibility;
        public byte VisibilityTeam { get; set; }

        protected override void ReadBody(ByteReader reader)
        {

            this.VisibilityTeam = reader.ReadByte();
        }
        protected override void WriteBody(ByteWriter writer)
        {
            writer.WriteByte(VisibilityTeam);
        }
    }
}
