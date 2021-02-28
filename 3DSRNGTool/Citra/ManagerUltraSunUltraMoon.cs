namespace Pk3DSRNGTool.Citra
{
    using Magnetosphere;

    public class ManagerUltraSunUltraMoon : Manager7
    {
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

        public ManagerUltraSunUltraMoon(IDeviceRW device)
            : base(device)
        {
        }
    }
}
