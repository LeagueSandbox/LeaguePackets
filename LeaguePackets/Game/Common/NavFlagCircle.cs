using System;
using System.Numerics;

namespace LeaguePackets.Game.Common
{
    public class NavFlagCircle
    {
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public uint Flags { get; set; }
    }

    public static class NavFlagCircleExtension
    {
        public static NavFlagCircle ReadNavFlagCircle(this ByteReader reader)
        {
            var data = new NavFlagCircle();
            data.Position = reader.ReadVector2();
            data.Radius = reader.ReadFloat();
            data.Flags = reader.ReadUInt32();
            return data;
        }

        public static void WriteNavFlagCircle(this ByteWriter writer, NavFlagCircle data)
        {
            if(data == null)
            {
                data = new NavFlagCircle();
            }
            writer.WriteVector2(data.Position);
            writer.WriteFloat(data.Radius);
            writer.WriteUInt32(data.Flags);
        }
    }
}
