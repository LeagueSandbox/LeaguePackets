﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.Game
{
    public class SPM_HierarchicalBBProfileUpdate : GamePacket // 0xB6
    {
        public override GamePacketID ID => GamePacketID.SPM_HierarchicalBBProfileUpdate;

        protected override void ReadBody(ByteReader reader)
        {
        }
        protected override void WriteBody(ByteWriter writer)
        {
        }
    }
}
