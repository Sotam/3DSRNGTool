namespace Pk3DSRNGTool.Citra
{
    using System;
    using Magnetosphere;

    public class ManagerXY : Manager6
    {
        public override ulong PartyAddress { get; } = 0x8CE1CF8;

        public override ulong WildAddress
        {
            get
            {
                /*
                def getWildOffset(self):
                    pointer = readDWord(self.citra, 0x880313c) - 0xA1C
                    if pointer < 0x8000000 or pointer > 0x8DF0000:
                        return 0x8805614
                    else:
                        pointer = readDWord(self.citra, pointer)
                        if pointer < 0x8000000 or pointer > 0x8DF0000:
                            return 0x8805614
                        else: 
                            return pointer
                */
                throw new NotImplementedException();
            }
        }

        public override ulong SeedAddress { get; } = 0x8c52844;

        public override ulong MtStart { get; } = 0x8c5284C;
        public override ulong MtIndex { get; } = 0x8c52848;
        public override ulong TinyStart { get; } = 0x8c52808;
        public override uint SaveVariable { get; } = 0x8C6A6A4;

        public override ulong EggReadyAddress { get; } = 0x8C80124;
        public override ulong EggAddress { get; } = 0x8c8012c;
        public override ulong Parent1Address { get; } = 0x8C7FF4C;
        public override ulong Parent2Address { get; } = 0x8C8003C;

        public ManagerXY(IDeviceRW device)
            : base(device)
        {
        }
    }
}