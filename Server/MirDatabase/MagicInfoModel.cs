using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Server.MirEnvir;
using S = ServerPackets;

namespace Server.MirDatabase {
    public class MagicInfoModel {
        protected static Envir Envir {
            get { return Envir.Main; }
        }

        public string Name;
        public Spell Spell;
        public byte BaseCost, LevelCost, Icon;
        public byte Level1, Level2, Level3;
        public ushort Need1, Need2, Need3;
        public uint DelayBase = 1800, DelayReduction;
        public ushort PowerBase, PowerBonus;
        public ushort MPowerBase, MPowerBonus;
        public float MultiplierBase = 1.0f, MultiplierBonus;
        public byte Range = 9;

        public override string ToString() => Name;

        public MagicInfoModel(string name, Spell spell, byte icon, byte level1, byte level2, byte level3, ushort need1, ushort need2, ushort need3, byte range) {
            Name = name;
            Spell = spell;
            Icon = icon;
            Level1 = level1;
            Level2 = level2;
            Level3 = level3;
            Need1 = need1;
            Need2 = need2;
            Need3 = need3;
            Range = range;
        }
        public MagicInfoModel(BinaryReader reader, int version = int.MaxValue, int customVersion = int.MaxValue) {
            Name = reader.ReadString();
            Spell = (Spell)reader.ReadByte();
            BaseCost = reader.ReadByte();
            LevelCost = reader.ReadByte();
            Icon = reader.ReadByte();
            Level1 = reader.ReadByte();
            Level2 = reader.ReadByte();
            Level3 = reader.ReadByte();
            Need1 = reader.ReadUInt16();
            Need2 = reader.ReadUInt16();
            Need3 = reader.ReadUInt16();
            DelayBase = reader.ReadUInt32();
            DelayReduction = reader.ReadUInt32();
            PowerBase = reader.ReadUInt16();
            PowerBonus = reader.ReadUInt16();
            MPowerBase = reader.ReadUInt16();
            MPowerBonus = reader.ReadUInt16();

            if (version > 66)
                Range = reader.ReadByte();
            if (version > 70) {
                MultiplierBase = reader.ReadSingle();
                MultiplierBonus = reader.ReadSingle();
            }
        }

        public void Save(BinaryWriter writer) {
            writer.Write(Name);
            writer.Write((byte)Spell);
            writer.Write(BaseCost);
            writer.Write(LevelCost);
            writer.Write(Icon);
            writer.Write(Level1);
            writer.Write(Level2);
            writer.Write(Level3);
            writer.Write(Need1);
            writer.Write(Need2);
            writer.Write(Need3);
            writer.Write(DelayBase);
            writer.Write(DelayReduction);
            writer.Write(PowerBase);
            writer.Write(PowerBonus);
            writer.Write(MPowerBase);
            writer.Write(MPowerBonus);
            writer.Write(Range);
            writer.Write(MultiplierBase);
            writer.Write(MultiplierBonus);
        }
    }

    public class UserMagic {
        protected static Envir Envir {
            get { return Envir.Main; }
        }

        public UserMagic() {
        }
        
        public Spell Spell;
        public MagicInfoModel InfoModel;

        public byte Level, Key;
        public ushort Experience;
        public bool IsTempSpell;
        public long CastTime;

        private MagicInfoModel GetMagicInfo(Spell spell) {
            for (int i = 0; i < Envir.MagicInfoList.Count; i++) {
                MagicInfoModel infoModel = Envir.MagicInfoList[i];
                if (infoModel.Spell != spell) continue;
                return infoModel;
            }
            return null;
        }

        public UserMagic(Spell spell) {
            Spell = spell;

            InfoModel = GetMagicInfo(Spell);
        }
        public UserMagic(BinaryReader reader) {
            Spell = (Spell)reader.ReadByte();
            InfoModel = GetMagicInfo(Spell);

            Level = reader.ReadByte();
            Key = reader.ReadByte();
            Experience = reader.ReadUInt16();

            if (Envir.LoadVersion < 15) return;
            IsTempSpell = reader.ReadBoolean();

            if (Envir.LoadVersion < 65) return;
            CastTime = reader.ReadInt64();
        }
        public void Save(BinaryWriter writer) {
            writer.Write((byte)Spell);

            writer.Write(Level);
            writer.Write(Key);
            writer.Write(Experience);
            writer.Write(IsTempSpell);
            writer.Write(CastTime);
        }

        public Packet GetInfo() {
            return new S.NewMagic {
                Magic = CreateClientMagic()
            };
        }

        public ClientMagic CreateClientMagic() {
            return new ClientMagic {
                Name = InfoModel.Name,
                Spell = Spell,
                BaseCost = InfoModel.BaseCost,
                LevelCost = InfoModel.LevelCost,
                Icon = InfoModel.Icon,
                Level1 = InfoModel.Level1,
                Level2 = InfoModel.Level2,
                Level3 = InfoModel.Level3,
                Need1 = InfoModel.Need1,
                Need2 = InfoModel.Need2,
                Need3 = InfoModel.Need3,
                Level = Level,
                Key = Key,
                Experience = Experience,
                IsTempSpell = IsTempSpell,
                Delay = GetDelay(),
                Range = InfoModel.Range,
                CastTime = (CastTime != 0) && (Envir.Time > CastTime) ? Envir.Time - CastTime : 0
            };
        }

        public int GetDamage(int DamageBase) => (int)((DamageBase + GetPower()) * GetMultiplier());

        public float GetMultiplier() => (InfoModel.MultiplierBase + (Level * InfoModel.MultiplierBonus));

        public int GetPower() => (int)Math.Round((MPower() / 4F) * (Level + 1) + DefPower());

        public int MPower() => 
            InfoModel.MPowerBonus > 0 ? Envir.Random.Next(InfoModel.MPowerBase, InfoModel.MPowerBonus + InfoModel.MPowerBase) : InfoModel.MPowerBase;
        
        public int DefPower() => 
            InfoModel.PowerBonus > 0 ? Envir.Random.Next(InfoModel.PowerBase, InfoModel.PowerBonus + InfoModel.PowerBase) : InfoModel.PowerBase;

        public int GetPower(int power) => (int)Math.Round(power / 4F * (Level + 1) + DefPower());

        public long GetDelay() => InfoModel.DelayBase - (Level * InfoModel.DelayReduction);
        
    }
}
