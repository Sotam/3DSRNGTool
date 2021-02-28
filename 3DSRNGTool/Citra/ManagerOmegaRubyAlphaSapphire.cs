namespace Pk3DSRNGTool.Citra
{
    using Magnetosphere;

    public class ManagerOmegaRubyAlphaSapphire : Manager6
    {
        public override ulong PartyAddress { get; } = 0x8CFB26C;
        public override ulong WildAddress { get; }
        public override ulong SeedAddress { get; } = 0x8c59e40;

        public override ulong MtStart { get; } = 0x8c59e48;
        public override ulong MtIndex { get; } = 0x8c59e44;

        public override ulong TinyStart { get; } = 0x8C59E04;
        public override uint SaveVariable { get; } = 0x8C71DB8;

        public override ulong EggReadyAddress { get; } = 0x8C88358;
        public override ulong EggAddress { get; } = 0x8C88360;

        public override ulong Parent1Address { get; } = 0x8C88180;
        public override ulong Parent2Address { get; } = 0x8C88270;

        public ManagerOmegaRubyAlphaSapphire(IDeviceRW device)
            : base(device)
        {
        }
    }
}