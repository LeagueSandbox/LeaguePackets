
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LeaguePackets.Game.Common;

namespace LeaguePackets.Game
{
    public class S2C_WriteNavFlags : GamePacket // 0x41
    {
        public override GamePacketID ID => GamePacketID.S2C_WriteNavFlags;
        public int SyncID { get; set; }
        public List<NavFlagCircle> NavFlagCircles { get; set; } = new List<NavFlagCircle>();

        protected override void ReadBody(ByteReader reader)
        {

            this.SyncID = reader.ReadInt32();
            int size = reader.ReadInt16();
            for (var i = 0; i < size; i += 16)
            {
                this.NavFlagCircles.Add(reader.ReadNavFlagCircle());
            }
        }
        protected override void WriteBody(ByteWriter writer)
        {
            writer.WriteInt32(SyncID);
            int size = NavFlagCircles.Count * 16;
            if(size > 0xFFFF)
            {
                throw new IOException("NavFlagCircles list too big!");   
            }
            for (int i = 0; i < NavFlagCircles.Count; i++)
            {
                writer.WriteNavFlagCircle(NavFlagCircles[i]);
            }
        }
    }
}
