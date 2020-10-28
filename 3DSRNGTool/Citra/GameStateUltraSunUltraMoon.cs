namespace Pk3DSRNGTool.Citra
{
    using System.Collections.Generic;
    using Magnetosphere;

    public class GameStateUltraSunUltraMoon : GameState
    {
        public GameStateUltraSunUltraMoon(IDeviceRW device) : base(device)
        {
            Main.SFMTAddressSeed = SeedAddress;
            Main.SFMTAddressStart = SfmtStart;
            Main.SFMTAddressIndex = SfmtIndex;

            Main.Initialize(Device);
        }

        public override ulong PartyAddress { get; } = 0x33F7FA44;
        public override ulong WildAddress { get; } = 0x3002F9A0;
        public override ulong SosAddress { get; } = 0x3002F9A0;

        public override ulong SeedAddress { get; } = 0x32663BF0;
        public override ulong SfmtStart { get; } = 0x330D35D8;
        public override ulong SfmtIndex { get; } = 0x330D3F98;

        public override ulong SosSeedAddress { get; } = 0x30038E30;
        public override ulong SosSFMTStart { get; } = 0x30038E30;
        public override ulong SosSFMTIndex { get; } = 0x300397F0;
        public override ulong SosChainLength { get; } = 0x300397F9;

        public override ulong EggReadyAddress { get; } = 0x3307B1E8;
        public override ulong EggAddress { get; } = 0x3307B1EC;
        public override ulong Parent1Address { get; } = 0x3307B011;
        public override ulong Parent2Address { get; } = 0x3307B0FA;

        public override IList<uint> GetEggSeeds()
        {
            var eggSeed0 = Device.ReadUInt32(EggAddress);
            var eggSeed1 = Device.ReadUInt32(EggAddress + 4);
            var eggSeed2 = Device.ReadUInt32(EggAddress + 8);
            var eggSeed3 = Device.ReadUInt32(EggAddress + 12);

            return new[] { eggSeed0, eggSeed1, eggSeed2, eggSeed3 };
        }
    }
}