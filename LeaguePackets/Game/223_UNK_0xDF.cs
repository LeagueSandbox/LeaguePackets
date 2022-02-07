
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.Game
{
    public class UNK_0xDF : GamePacket // 0xDF
    {
        public override GamePacketID ID => GamePacketID.Unknown_223;

        protected override void ReadBody(ByteReader reader) 
        {
        }
        protected override void WriteBody(ByteWriter writer) { }
    }
}
