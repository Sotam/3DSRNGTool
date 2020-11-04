namespace Pk3DSRNGTool.Citra
{
    using System.ComponentModel;
    using Exceptions;
    using Magnetosphere;
    using RNG;

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RNGState
    {
        internal ulong SFMTAddressSeed { private get; set; }
        internal ulong SFMTAddressStart { private get; set; }
        internal ulong SFMTAddressIndex { private get; set; }

        public uint InitialSeed { get; private set; }
        public ulong CurrentSeed { get; private set; }
        public int FrameCount { get; private set; } = -1;
        public int FrameDifference { get; private set; } = -1;

        private SFMT SFMT { get; set; }
        private IDeviceRW Device { get; set; }

        public void Initialize(IDeviceRW device)
        {
            Device = device;
            InitialSeed = Device.ReadUInt32(SFMTAddressSeed);
            SFMT = new SFMT(InitialSeed);
            CurrentSeed = InitialSeed;
            FrameCount = -1;
        }

        public void UpdateFrame()
        {
            try
            {
                var game = CalcCurrentSeed();
                var seed = CurrentSeed;

                var frameCount = 0;
                var frameDifference = FrameCount;

                while (game != seed)
                {
                    seed = SFMT.Nextulong();
                    frameCount++;

                    if (frameCount > 5000000)
                        throw new FrameOutOfRangeException();
                }

                FrameCount += frameCount;

                var difference = FrameCount - frameDifference;
                if (difference != 0)
                    FrameDifference = difference;

                CurrentSeed = seed;
            }
            catch (FrameOutOfRangeException)
            {
                SFMT = new SFMT(InitialSeed);
                FrameCount = -1;
                FrameDifference = -1;
            }
        }

        private ulong CalcCurrentSeed()
        {
            var index = Device.ReadUInt32(SFMTAddressIndex);
            var pointer = GetSFMTPointer(index);
            var seed1 = Device.ReadUInt32(pointer);
            var seed2 = Device.ReadUInt32(pointer + 4);

            return ((ulong)seed2 << 32) | seed1;
        }

        private ulong GetSFMTPointer(uint index)
        {
            ulong pointer;
            if (index == 624)
                pointer = SFMTAddressStart;
            else
                pointer = SFMTAddressStart + (index * 4);
            return pointer;
        }
    }
}
