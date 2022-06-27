namespace LeaguePackets.Game.Events
{
    public abstract class ArgsForClient: ArgsBase
    {
        public uint ScriptNameHash { get; set; }
        public byte EventSource { get; set; }
        // FIXME: new byte appeared here
        public byte Unknown { get; set; }
        public uint SourceObjectNetID { get; set; }
        public uint ParentScriptNameHash { get; set; }
        public uint ParentCasterNetID { get; set; }
        public ushort Bitfield { get; set; }
    }
}