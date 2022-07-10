using LeaguePackets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeaguePackets.Game;
using GameServerCore.Enums;

namespace LeaguePacketsSerializer
{
    class Program
    {
        [Flags]
        public enum ENetPacketFlags
        {
            Reliable = (1 << 7),
            Unsequenced = (1 << 6),
            ReliableUnsequenced = Reliable | Unsequenced,
            None = 0,
        }
        public class ENetPacket
        {
            public float Time { get; set; }
            public byte[] Bytes { get; set; }
            public byte Channel { get; set; }
            public ENetPacketFlags Flags { get; set; }
        }

        public class SerializedPacket
        {
            public int RawID { get; set; }
            public object Packet { get; set; }
            public float Time { get; set; }
            [JsonConverter(typeof(StringEnumConverter))]
            public ChannelID? ChannelID { get; set; }
            public byte RawChannel { get; set; }
        }

        public class BadPacket
        {
            public int RawID { get; set; }
            public byte[] Raw { get; set; }
            public byte RawChannel { get; set; }
            public string Error { get; set; }
        }

        public static void SerializeToFile(object what, string fileName)
        {
            using (StreamWriter file = File.CreateText(fileName))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.TypeNameHandling = TypeNameHandling.Auto;
                serializer.Serialize(file, what);
            }
        }

        private enum ReplicationType {
            Unknown = 0,
            Turret = 1,
            Building = 2,
            Hero = 3,
            Monster = 4,
            Pet = 4,
            Minion = 4,
            LaneMinion = 4
        }
        static Dictionary<uint, ReplicationType> replicationTypes =
           new Dictionary<uint, ReplicationType>();
        static void SetReplicationType(BasePacket packet)
        {
            if(packet is S2C_CreateTurret ct)
            {
                replicationTypes[ct.NetID] = ReplicationType.Turret;
            }
            else if(packet is S2C_CreateHero ch)
            {
                replicationTypes[ch.NetID] = ReplicationType.Hero;
            }
            else if(packet is S2C_CreateNeutral cn)
            {
                replicationTypes[cn.NetID] = ReplicationType.Monster;
            }
            else if(packet is CHAR_SpawnPet sp)
            {
                //TODO: verify
                replicationTypes[sp.SenderNetID] = ReplicationType.Pet;
            }
            else if(packet is SpawnMinionS2C sm)
            {
                replicationTypes[sm.NetID] = ReplicationType.Minion;
            }
            else if(packet is Barrack_SpawnUnit su)
            {
                //TODO: verify
                replicationTypes[su.SenderNetID] = ReplicationType.LaneMinion;
            }
            else if(packet is OnEnterVisibilityClient vp)
            {
                if(vp.SenderNetID >= 0xFF000000)
                {
                    replicationTypes[vp.SenderNetID] = ReplicationType.Building;
                }
                foreach(var subpacket in vp.Packets)
                {
                    SetReplicationType(subpacket);
                }
            }
        }

        public class FakeOnReplication
        {
            // BasePacket
            public byte[] ExtraBytes;
            // GamePacket
            public uint SenderNetID;
            // OnReplication
            public GamePacketID ID = GamePacketID.OnReplication;
            public uint SyncID;
            // replaced field
            public List<FakeReplicationData> ReplicationData =
               new List<FakeReplicationData>();
        }

        public class FakeReplicationData
        {
            public uint UnitNetID;
            public Dictionary<string, object> Data;
            public FakeReplicationData(uint netID, Dictionary<string, object> data)
            {
                UnitNetID = netID;
                Data = data;
            }
        }

        class Replicate
        {
            public uint Uint;
            public float Float;
            public bool IsFloat;
            public Replicate(uint value)
            {
                Uint = value;
                IsFloat = false;
            }
            public Replicate(float value)
            {
                Float = value;
                IsFloat = true;
            }
        }

        static uint u;
        static float f;
        static bool recording;
        static bool?[][,] isFloatArray;
        static Replicate[,] currentValues;
        static ReplicationType currentReplicationType;

        static bool TryGet(int primaryId, int secondaryId, bool isFloat)
        {
            //TODO: value.isFloat != isFloat
            if(recording)
            {
                isFloatArray[(int)currentReplicationType][primaryId, secondaryId] = isFloat;
                return false;
            }
            Replicate value = currentValues[primaryId, secondaryId];
            if(value == null)
            {
                return false;
            }
            if(isFloat)
            {
                f = value.Float;
            }
            else
            {
                u = value.Uint;
            }
            return true;
        }

        static bool TryGetUint(int primaryId, int secondaryId)
        {
            return TryGet(primaryId, secondaryId, false);
        }

        static bool TryGetFloat(int primaryId, int secondaryId)
        {
            return TryGet(primaryId, secondaryId, true);
        }

        static Dictionary<string, object> GenDataDict(ReplicationType replicationType, Replicate[,] values)
        {
            var data = new Dictionary<string, object>();
            currentReplicationType = replicationType;
            currentValues = values;

            switch(replicationType)
            {
                // UpdateFloat\((.*), (\d+), (\d+)\) -> if(TryGetFloat($2, $3)) data["$1"] = f
                // UpdateUint\((.*), (\d+), (\d+)\) -> if(TryGetUint($2, $3)) data["$1"] = u
                // UpdateBool\((.*), (\d+), (\d+)\) -> if(TryGetUint($2, $3)) data["$1"] = u == 1u

                case ReplicationType.Turret:
            
                /**/ if(TryGetFloat(1, 0)) data["Stats.ManaPoints.Total"] = f; //mMaxMP
                /**/ if(TryGetFloat(1, 1)) data["Stats.CurrentMana"] = f; //mMP
                if(TryGetUint(1, 2)) data["Stats.ActionState"] = ((ActionState)u).ToString(); //ActionState
                if(TryGetUint(1, 3)) data["Stats.IsMagicImmune"] = u == 1u; //MagicImmune
                if(TryGetUint(1, 4)) data["Stats.IsInvulnerable"] = u == 1u; //IsInvulnerable
                if(TryGetUint(1, 5)) data["Stats.IsPhysicalImmune"] = u == 1u; //IsPhysicalImmune
                if(TryGetUint(1, 6)) data["Stats.IsLifestealImmune"] = u == 1u; //IsLifestealImmune
                if(TryGetFloat(1, 7)) data["Stats.AttackDamage.BaseValue"] = f; //mBaseAttackDamage
                if(TryGetFloat(1, 8)) data["Stats.Armor.Total"] = f; //mArmor
                if(TryGetFloat(1, 9)) data["Stats.MagicResist.Total"] = f; //mSpellBlock
                if(TryGetFloat(1, 10)) data["Stats.AttackSpeedMultiplier.Total"] = f; //mAttackSpeedMod
                if(TryGetFloat(1, 11)) data["Stats.AttackDamage.FlatBonus"] = f; //mFlatPhysicalDamageMod
                if(TryGetFloat(1, 12)) data["Stats.AttackDamage.PercentBonus"] = f; //mPercentPhysicalDamageMod
                if(TryGetFloat(1, 13)) data["Stats.AbilityPower.Total"] = f; //mFlatMagicDamageMod
                if(TryGetFloat(1, 14)) data["Stats.HealthRegeneration.Total"] = f; //mHPRegenRate
                if(TryGetFloat(3, 0)) data["Stats.CurrentHealth"] = f; //mHP
                if(TryGetFloat(3, 1)) data["Stats.HealthPoints.Total"] = f; //mMaxHP
                /**/ if(TryGetFloat(3, 2)) data["Stats.PerceptionRange.FlatBonus"] = f; //mFlatBubbleRadiusMod
                /**/ if(TryGetFloat(3, 3)) data["Stats.PerceptionRange.PercentBonus"] = f; //mPercentBubbleRadiusMod
                if(TryGetFloat(3, 4)) data["Stats.GetTrueMoveSpeed()"] = f; //mMoveSpeed
                if(TryGetFloat(3, 5)) data["Stats.Size.Total"] = f; //mSkinScaleCoef(mistyped as mCrit)
                if(TryGetUint(5, 0)) data["Stats.IsTargetable"] = u == 1u; //mIsTargetable
                if(TryGetUint(5, 1)) data["Stats.IsTargetableToTeam"] = ((SpellDataFlags)u).ToString(); //mIsTargetableToTeamFlags

                break;
            
                case ReplicationType.Building:
            
                if(TryGetFloat(1, 0)) data["Stats.CurrentHealth"] = f; //mHP
                if(TryGetUint(1, 1)) data["Stats.IsInvulnerable"] = u == 1u; //IsInvulnerable
                if(TryGetUint(5, 0)) data["Stats.IsTargetable"] = u == 1u; //mIsTargetable
                if(TryGetUint(5, 1)) data["Stats.IsTargetableToTeam"] = ((SpellDataFlags)u).ToString(); //mIsTargetableToTeamFlags

                break;
                
                case ReplicationType.Hero:
                
                if(TryGetFloat(0, 0)) data["Stats.Gold"] = f; //mGold
                /**/ if(TryGetFloat(0, 1)) data["Stats.TotalGold"] = f; //mGoldTotal
                if(TryGetUint(0, 2)) data["(uint)Stats.SpellsEnabled"] = u; //mReplicatedSpellCanCastBitsLower1
                if(TryGetUint(0, 3)) data["(uint)(Stats.SpellsEnabled >> 32)"] = u; //mReplicatedSpellCanCastBitsUpper1
                if(TryGetUint(0, 4)) data["(uint)Stats.SummonerSpellsEnabled"] = u; //mReplicatedSpellCanCastBitsLower2
                if(TryGetUint(0, 5)) data["(uint)(Stats.SummonerSpellsEnabled >> 32)"] = u; //mReplicatedSpellCanCastBitsUpper2
                /**/ if(TryGetUint(0, 6)) data["Stats.EvolvePoints"] = u; //mEvolvePoints
                /**/ if(TryGetUint(0, 7)) data["Stats.EvolveFlags"] = u; //mEvolveFlag
                for (var i = 0; i < 4; i++)
                {
                    if(TryGetFloat(0, 8 + i)) data[$"Stats.ManaCost[{i}]"] = f; //ManaCost_{i}
                }
                for(var i = 0; i < 16; i++)
                {
                    if(TryGetFloat(0, 12 + i)) data[$"Stats.ManaCost[{45 + i}]"] = f; //ManaCost_Ex{i}
                }
                if(TryGetUint(1, 0)) data["Stats.ActionState"] = ((ActionState)u).ToString();
                if(TryGetUint(1, 1)) data["Stats.IsMagicImmune"] = u == 1u; //MagicImmune
                if(TryGetUint(1, 2)) data["Stats.IsInvulnerable"] = u == 1u; //IsInvulnerable
                if(TryGetUint(1, 3)) data["Stats.IsPhysicalImmune"] = u == 1u; //IsPhysicalImmune
                if(TryGetUint(1, 4)) data["Stats.IsLifestealImmune"] = u == 1u; //IsLifestealImmune
                if(TryGetFloat(1, 5)) data["Stats.AttackDamage.BaseValue"] = f; //mBaseAttackDamage
                if(TryGetFloat(1, 6)) data["Stats.AbilityPower.BaseValue"] = f; //mBaseAbilityDamage
                /**/ if(TryGetFloat(1, 7)) data["Stats.DodgeChance"] = f; //mDodge
                if(TryGetFloat(1, 8)) data["Stats.CriticalChance.Total"] = f; //mCrit
                if(TryGetFloat(1, 9)) data["Stats.Armor.Total"] = f; //mArmor
                if(TryGetFloat(1, 10)) data["Stats.MagicResist.Total"] = f; //mSpellBlock
                if(TryGetFloat(1, 11)) data["Stats.HealthRegeneration.Total"] = f; //mHPRegenRate
                if(TryGetFloat(1, 12)) data["Stats.ManaRegeneration.Total"] = f; //mPARRegenRate
                if(TryGetFloat(1, 13)) data["Stats.Range.Total"] = f; //mAttackRange
                if(TryGetFloat(1, 14)) data["Stats.AttackDamage.FlatBonus"] = f; //mFlatPhysicalDamageMod
                if(TryGetFloat(1, 15)) data["Stats.AttackDamage.PercentBonus"] = f; //mPercentPhysicalDamageMod
                if(TryGetFloat(1, 16)) data["Stats.AbilityPower.FlatBonus"] = f; //mFlatMagicDamageMod
                /**/ if(TryGetFloat(1, 17)) data["Stats.MagicResist.FlatBonus"] = f; //mFlatMagicReduction
                /**/ if(TryGetFloat(1, 18)) data["Stats.MagicResist.PercentBonus"] = f; //mPercentMagicReduction
                if(TryGetFloat(1, 19)) data["Stats.AttackSpeedMultiplier.Total"] = f; //mAttackSpeedMod
                if(TryGetFloat(1, 20)) data["Stats.Range.FlatBonus"] = f; //mFlatCastRangeMod
                // TODO: Find out why a negative value is required for ability cooldowns to display properly.
                if(TryGetFloat(1, 21)) data["Stats.CooldownReduction.Total"] = -f; //mPercentCooldownMod
                /**/ if(TryGetFloat(1, 22)) data["Stats.PassiveCooldownEndTime"] = f; //mPassiveCooldownEndTime
                /**/ if(TryGetFloat(1, 23)) data["Stats.PassiveCooldownTotalTime"] = f; //mPassiveCooldownTotalTime
                if(TryGetFloat(1, 24)) data["Stats.ArmorPenetration.FlatBonus"] = f; //mFlatArmorPenetration
                if(TryGetFloat(1, 25)) data["Stats.ArmorPenetration.PercentBonus"] = f; //mPercentArmorPenetration
                if(TryGetFloat(1, 26)) data["Stats.MagicPenetration.FlatBonus"] = f; //mFlatMagicPenetration
                if(TryGetFloat(1, 27)) data["Stats.MagicPenetration.PercentBonus"] = f; //mPercentMagicPenetration
                if(TryGetFloat(1, 28)) data["Stats.LifeSteal.Total"] = f; //mPercentLifeStealMod
                if(TryGetFloat(1, 29)) data["Stats.SpellVamp.Total"] = f; //mPercentSpellVampMod
                if(TryGetFloat(1, 30)) data["Stats.Tenacity.Total"] = f; //mPercentCCReduction
                if(TryGetFloat(2, 0)) data["Stats.Armor.PercentBonus"] = f; //mPercentBonusArmorPenetration
                if(TryGetFloat(2, 1)) data["Stats.MagicPenetration.PercentBonus"] = f; //mPercentBonusMagicPenetration
                /**/ if(TryGetFloat(2, 2)) data["Stats.HealthRegeneration.BaseValue"] = f; //mBaseHPRegenRate
                /**/ if(TryGetFloat(2, 3)) data["Stats.ManaRegeneration.BaseValue"] = f; //mBasePARRegenRate
                if(TryGetFloat(3, 0)) data["Stats.CurrentHealth"] = f; //mHP
                if(TryGetFloat(3, 1)) data["Stats.CurrentMana"] = f; //mMP
                if(TryGetFloat(3, 2)) data["Stats.HealthPoints.Total"] = f; //mMaxHP
                if(TryGetFloat(3, 3)) data["Stats.ManaPoints.Total"] = f; //mMaxMP
                if(TryGetFloat(3, 4)) data["Stats.Experience"] = f; //mExp
                /**/ if(TryGetFloat(3, 5)) data["Stats.LifeTime"] = f; //mLifetime
                /**/ if(TryGetFloat(3, 6)) data["Stats.MaxLifeTime"] = f; //mMaxLifetime
                /**/ if(TryGetFloat(3, 7)) data["Stats.LifeTimeTicks"] = f; //mLifetimeTicks
                /**/ if(TryGetFloat(3, 8)) data["Stats.PerceptionRange.FlatMod"] = f; //mFlatBubbleRadiusMod
                /**/ if(TryGetFloat(3, 9)) data["Stats.PerceptionRange.PercentMod"] = f; //mPercentBubbleRadiusMod
                if(TryGetFloat(3, 10)) data["Stats.GetTrueMoveSpeed()"] = f; //mMoveSpeed
                if(TryGetFloat(3, 11)) data["Stats.Size.Total"] = f; //mSkinScaleCoef(mistyped as mCrit)
                /**/ if(TryGetFloat(3, 12)) data["Stats.FlatPathfindingRadiusMod"] = f; //mPathfindingRadiusMod
                if(TryGetUint(3, 13)) data["Stats.Level"] = u; //mLevelRef
                if(TryGetUint(3, 14)) data["Owner.MinionCounter"] = u; //mNumNeutralMinionsKilled
                if(TryGetUint(3, 15)) data["Stats.IsTargetable"] = u == 1u; //mIsTargetable
                if(TryGetUint(3, 16)) data["Stats.IsTargetableToTeam"] = ((SpellDataFlags)u).ToString(); //mIsTargetableToTeamFlags

                break;
                
                case ReplicationType.Minion:
                //case ReplicationType.LaneMinion:
                //case ReplicationType.Monster:
                //case ReplicationType.Pet:

                if(TryGetFloat(1, 0)) data["Stats.CurrentHealth"] = f; //mHP
                if(TryGetFloat(1, 1)) data["Stats.HealthPoints.Total"] = f; //mMaxHP
                /**/ if(TryGetFloat(1, 2)) data["Stats.LifeTime"] = f; //mLifetime
                /**/ if(TryGetFloat(1, 3)) data["Stats.MaxLifeTime"] = f; //mMaxLifetime
                /**/ if(TryGetFloat(1, 4)) data["Stats.LifeTimeTicks"] = f; //mLifetimeTicks
                if(TryGetFloat(1, 5)) data["Stats.ManaPoints.Total"] = f; //mMaxMP
                if(TryGetFloat(1, 6)) data["Stats.CurrentMana"] = f; //mMP
                if(TryGetUint(1, 7)) data["Stats.ActionState"] = ((ActionState)u).ToString(); //ActionState
                if(TryGetUint(1, 8)) data["Stats.IsMagicImmune"] = u == 1u; //MagicImmune
                if(TryGetUint(1, 9)) data["Stats.IsInvulnerable"] = u == 1u; //IsInvulnerable
                if(TryGetUint(1, 10)) data["Stats.IsPhysicalImmune"] = u == 1u; //IsPhysicalImmune
                if(TryGetUint(1, 11)) data["Stats.IsLifestealImmune"] = u == 1u; //IsLifestealImmune
                if(TryGetFloat(1, 12)) data["Stats.AttackDamage.BaseValue"] = f; //mBaseAttackDamage
                if(TryGetFloat(1, 13)) data["Stats.Armor.Total"] = f; //mArmor
                if(TryGetFloat(1, 14)) data["Stats.MagicResist.Total"] = f; //mSpellBlock
                if(TryGetFloat(1, 15)) data["Stats.AttackSpeedMultiplier.Total"] = f; //mAttackSpeedMod
                if(TryGetFloat(1, 16)) data["Stats.AttackDamage.FlatBonus"] = f; //mFlatPhysicalDamageMod
                if(TryGetFloat(1, 17)) data["Stats.AttackDamage.PercentBonus"] = f; //mPercentPhysicalDamageMod
                if(TryGetFloat(1, 18)) data["Stats.AbilityPower.Total"] = f; //mFlatMagicDamageMod
                if(TryGetFloat(1, 19)) data["Stats.HealthRegeneration.Total"] = f; //mHPRegenRate
                if(TryGetFloat(1, 20)) data["Stats.ManaRegeneration.Total"] = f; //mPARRegenRate
                if(TryGetFloat(1, 21)) data["Stats.MagicResist.FlatBonus"] = f; //mFlatMagicReduction
                if(TryGetFloat(1, 22)) data["Stats.MagicResist.PercentBonus"] = f; //mPercentMagicReduction
                /**/ if(TryGetFloat(3, 0)) data["Stats.PerceptionRange.FlatBonus"] = f; //mFlatBubbleRadiusMod
                /**/ if(TryGetFloat(3, 1)) data["Stats.PerceptionRange.PercentBonus"] = f; //mPercentBubbleRadiusMod
                if(TryGetFloat(3, 2)) data["Stats.GetTrueMoveSpeed()"] = f; //mMoveSpeed
                if(TryGetFloat(3, 3)) data["Stats.Size.Total"] = f; //mSkinScaleCoef(mistyped as mCrit)
                if(TryGetUint(3, 4)) data["Stats.IsTargetable"] = u == 1u; //mIsTargetable
                if(TryGetUint(3, 5)) data["Stats.IsTargetableToTeam"] = ((SpellDataFlags)u).ToString(); //mIsTargetableToTeamFlags

                break;
            }

            return data;
        }

        static void DumpState(byte[] bytes, int i, ReplicationType replicationType, byte primaryId, byte secondaryId)
        {
            Console.WriteLine(
                $"bytes = new byte[{bytes.Length}]{{ {string.Join(", ", bytes)} }}; i = {i};" +
                $"type = {replicationType}; primaryId = {primaryId}; secondaryId = {secondaryId}"
            );                                
        }

        static void Main(string[] args)
        {
            recording = true;
            var types = (ReplicationType[])Enum.GetValues(typeof(ReplicationType));
            isFloatArray = new bool?[types.Length][,];
            foreach (var type in types)
            {
                isFloatArray[(int)type] = new bool?[6, 32];
                GenDataDict(type, null);
            }
            recording = false;

            var fileName = "test.rlp.json";
            if (args.Length > 0)
                fileName = args[0];
            Console.WriteLine("Reading file...");
            var json = File.ReadAllText(fileName);
            Console.WriteLine("Parsing json...");
            var rawPackets = JsonConvert.DeserializeObject<List<ENetPacket>>(json);
            var serializedPackets = new List<SerializedPacket>();
            var hardBadPackets = new List<BadPacket>();
            var softBadPackets = new List<BadPacket>();
            Console.WriteLine("Processing raw packets...");
            foreach (var rPacket in rawPackets)
            {
                if (rPacket.Channel < 8)
                {
                    int rawID = rPacket.Bytes[0];
                    if (rawID == 254)
                    {
                        rawID = rPacket.Bytes[5] | rPacket.Bytes[6] << 8;
                    }
                    try
                    {
                        var packet = BasePacket.Create(rPacket.Bytes, (ChannelID)rPacket.Channel);
                        object packetToSerialize = packet;

                        if(packet is OnReplication or)
                        {
                            var p = new FakeOnReplication();
                            p.SyncID = or.SyncID;
                            p.SenderNetID = or.SenderNetID;
                            p.ExtraBytes = or.ExtraBytes;
                            foreach(var rd in or.ReplicationData)
                            {
                                uint netID = rd.UnitNetID;
                                var values = new Replicate[6, 32];
                                var replicationType = replicationTypes.GetValueOrDefault(netID, ReplicationType.Unknown);

                                if(replicationType == ReplicationType.Unknown)
                                {
                                    Console.WriteLine($"Warning: the type of #{netID} is unknown");
                                    continue;
                                }

                                for (byte primaryId = 0; primaryId < 6; primaryId++)
                                {
                                    uint secondaryIdArray = rd.Data[primaryId].Item1;
                                    if(secondaryIdArray == 0)
                                    {
                                        continue;
                                    }
                                    int i = 0;
                                    var bytes = rd.Data[primaryId].Item2;

                                    for (byte secondaryId = 0; secondaryId < 32; secondaryId++)
                                    {
                                        if(((secondaryIdArray >> secondaryId) & 1) == 0)
                                        {
                                            continue;
                                        }
                                        
                                        bool? isFloat = isFloatArray[(int)replicationType][primaryId, secondaryId];
                                        if(isFloat == null)
                                        {
                                            Console.WriteLine($"Warning: the type for [{replicationType}][{primaryId}, {secondaryId}] is unknown");
                                            DumpState(bytes, i, replicationType, primaryId, secondaryId);
                                            break;
                                        }
                                        else if(isFloat == true)
                                        {
                                            try
                                            {  
                                                float value = 0;
                                                if(bytes[i] == 0xFF)
                                                {
                                                    i++;
                                                }
                                                else
                                                {
                                                    int startIndex = i;
                                                    if(bytes[i] == 0xFE)
                                                    {
                                                        startIndex++;
                                                    }
                                                    value = BitConverter.ToSingle(
                                                        bytes,
                                                        startIndex
                                                    );
                                                    i = startIndex + 4;
                                                }
                                                values[primaryId, secondaryId] = new Replicate(value);
                                            }
                                            catch(Exception e)
                                            {
                                                DumpState(bytes, i, replicationType, primaryId, secondaryId);
                                            }
                                        }
                                        else
                                        {
                                            try
                                            {
                                                uint value = 0;
                                                int j = 0;
                                                for(; (bytes[i] & 0x80) != 0; i++, j += 7)
                                                {
                                                    value |= ((uint)bytes[i] & 0x7f) << j;
                                                }
                                                value |= (uint)bytes[i] << j;
                                                i++;
                                                values[primaryId, secondaryId] = new Replicate(value);
                                            }
                                            catch(Exception e)
                                            {
                                                DumpState(bytes, i, replicationType, primaryId, secondaryId);
                                            }
                                        }
                                    }
                                }
                                p.ReplicationData.Add(new FakeReplicationData(netID, GenDataDict(replicationType, values)));
                            }
                            packetToSerialize = p;
                        }
                        else
                        {
                            SetReplicationType(packet);
                        }

                        serializedPackets.Add(new SerializedPacket
                        {
                            RawID = rawID,
                            Packet = packetToSerialize,
                            Time = rPacket.Time,
                            ChannelID = rPacket.Channel < 8 ? (ChannelID)rPacket.Channel : (ChannelID?)null,
                            RawChannel = rPacket.Channel,
                        });
                        if (rPacket.Channel > 0 && packet.ExtraBytes.Length > 0)
                        {
                            softBadPackets.Add(new BadPacket()
                            {
                                RawID = rawID,
                                Raw = rPacket.Bytes,
                                RawChannel = rPacket.Channel,
                                Error = $"Extra bytes: {Convert.ToBase64String(packet.ExtraBytes)}",
                            });
                        }
                        if(packet is IGamePacketsList list)
                        {
                            foreach(var packet2 in list.Packets)
                            {
                                if (rPacket.Channel > 0 && packet2.ExtraBytes.Length > 0)
                                {
                                    softBadPackets.Add(new BadPacket()
                                    {
                                        RawID = (int)packet2.ID,
                                        Raw = rPacket.Bytes,
                                        RawChannel = rPacket.Channel,
                                        Error = $"Extra bytes in {packet2.GetType().Name}: {Convert.ToBase64String(packet2.ExtraBytes)}",
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        hardBadPackets.Add(new BadPacket()
                        {
                            RawID = rawID,
                            Raw = rPacket.Bytes,
                            RawChannel = rPacket.Channel,
                            Error = exception.ToString(),
                        });
                    }
                }

            }

            Console.WriteLine($"Processed! Good: {serializedPackets.Count}, Soft Error: {softBadPackets.Count}, Hard Error: {hardBadPackets.Count}");
            Console.WriteLine($"Soft bad IDs:{string.Join(",", softBadPackets.Select(x => x.RawID.ToString()).Distinct())}");
            Console.WriteLine($"Hard bad IDs:{string.Join(",", hardBadPackets.Select(x => x.RawID.ToString()).Distinct())}");

            Console.WriteLine("Writing hard bad to file .hardbad.json");
            SerializeToFile(hardBadPackets, fileName.Replace(".rlp.json", ".rlp.hardbad.json"));

            Console.WriteLine("Writing soft bad to file .softbad.json");
            SerializeToFile(softBadPackets, fileName.Replace(".rlp.json", ".rlp.softbad.json"));

            Console.WriteLine("Writing serialized to .rlp.serialized.json...");
            SerializeToFile(serializedPackets, fileName.Replace(".rlp.json", ".rlp.serialized.json"));

            Console.WriteLine("Done!");
            return;
        }
    }
}
