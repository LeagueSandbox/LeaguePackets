﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.Game
{
    public class OnLeaveLocalVisibilityClient : GamePacket // 0x35
    {
        public override GamePacketID ID => GamePacketID.OnLeaveLocalVisibilityClient;

        protected override void ReadBody(ByteReader reader) 
        {
        }
        protected override void WriteBody(ByteWriter writer) {}
    }
}
