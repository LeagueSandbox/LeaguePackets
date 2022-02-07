
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.Game
{
    public class S2C_TeamUpdateDragonBuffCount : GamePacket // 0x12C
    {
        public override GamePacketID ID => GamePacketID.S2C_TeamUpdateDragonBuffCount;
        public bool TeamIsOrder { get; set; }
        // If this is not 0 the rest doesn't get processed
        public uint Unknown2 { get; set; }
        // Dragon kills/buff count
        public uint Count { get; set; }

        protected override void ReadBody(ByteReader reader)
        {

            byte bitfield = reader.ReadByte();
            TeamIsOrder = (bitfield & 0x01) != 0;

            Unknown2 = reader.ReadUInt32();
            Count = reader.ReadUInt32();
        }
        protected override void WriteBody(ByteWriter writer)
        {
            byte bitfield = 0;
            if (TeamIsOrder)
                bitfield |= 0x01;
            writer.WriteByte(bitfield);

            writer.WriteUInt32(Unknown2);
            writer.WriteUInt32(Count);
        }
    }
}
