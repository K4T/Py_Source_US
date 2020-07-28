using Py_Game.Defines;
using System;
using System.Runtime.InteropServices;

namespace Py_Game.Game.Data
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GameInformation
    {
        public byte Unknown1;
        public uint VSTime;
        public uint GameTime;
        public byte MaxPlayer;
        public GAME_TYPE GameType;
        public byte HoleTotal;
        public byte Map;
        public byte Mode;
        public uint NaturalMode;
        public bool GMEvent;
        // Hole Repeater
        public byte HoleNumber;
        public UInt32 LockHole;

        // Game Data
        public string Name;
        public string Password;
        public UInt32 Artifact;
        // Grandprix
        public bool GP;
        public UInt32 GPTypeID;
        public UInt32 GPTypeIDA;
        public UInt32 GPTime;
        public DateTime GPStart;
        public byte Time30S;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GameDataInfo
    {
        public byte Unknown1 { get; set; }
        public uint VSTime { get; set; }
        public uint GameTime { get; set; }
        public byte MaxPlayer { get; set; }
        public GAME_TYPE GameType { get; set; }
        public byte HoleTotal { get; set; }
        public byte Map { get; set; }
        public byte Mode { get; set; }
        public uint NaturalMode { get; set; }
        public bool GMEvent;
        // Hole Repeater
        public byte HoleNumber;
        public UInt32 LockHole;

        // Game Data
        public string Name;
        public string Password;
        public UInt32 Artifact;
        // Grandprix
        public bool GP;
        public UInt32 GPTypeID;
        public UInt32 GPTypeIDA;
        public UInt32 GPTime;
        public DateTime GPStart;
        public byte Time30S;
    }
    

    public class GameHoleInfo
    {
        public byte Hole;
        public byte Weather;
        public ushort WindPower;
        public ushort WindDirection;
        public byte Map;
        public byte Pos;
    }
}
