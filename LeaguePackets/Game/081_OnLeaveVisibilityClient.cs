
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.Game
{
    public class OnLeaveVisibilityClient : GamePacket // 0x51
    {
        public override GamePacketID ID => GamePacketID.OnLeaveVisibilityClient;

        protected override void ReadBody(ByteReader reader) 
        {
        }
        protected override void WriteBody(ByteWriter writer) {}
    }
}
