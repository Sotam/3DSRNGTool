namespace Pk3DSRNGTool.Citra
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;
    using Magnetosphere;
    using PKHeX.Core;
    using RNG;

    public abstract class Manager7 : IManager
    {
        private readonly IDeviceRW _device;

        private SFMT SFMT { get; set; }

        public abstract ulong PartyAddress { get; }
        public abstract ulong WildAddress { get; }
        public abstract ulong SosAddress { get; }

        public abstract ulong SeedAddress { get; }
        public abstract ulong SfmtStart { get; }
        public abstract ulong SfmtIndex { get; }

        public abstract ulong SosSeedAddress { get; }
        public abstract ulong SosSFMTStart { get; }
        public abstract ulong SosSFMTIndex { get; }
        public abstract ulong SosChainLength { get; }

        public abstract ulong EggReadyAddress { get; }
        public abstract ulong EggAddress { get; }
        public abstract ulong Parent1Address { get; }
        public abstract ulong Parent2Address { get; }

        public uint InitialSeed { get; }
        public ulong CurrentSeed { get; private set; }
        public int FrameCount { get; private set; } = -1;
        public int FrameDifference { get; private set; } = -1;

        public uint GetSaveVariable => throw new NotImplementedException();
        public uint GetTimeVariable => throw new NotImplementedException();
        public IList<uint> GetTinyMT => throw new NotImplementedException();

        protected Manager7(IDeviceRW device)
        {
            _device = device;

            InitialSeed = _device.ReadUInt32(SeedAddress);
            SFMT = new SFMT(InitialSeed);
            CurrentSeed = InitialSeed;
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
            var index = _device.ReadUInt32(SfmtIndex);
            var pointer = GetSFMTPointer(index);
            var seed1 = _device.ReadUInt32(pointer);
            var seed2 = _device.ReadUInt32(pointer + 4);

            return ((ulong)seed2 << 32) | seed1;
        }

        private ulong GetSFMTPointer(uint index)
        {
            ulong pointer;
            if (index == 624)
                pointer = SfmtStart;
            else
                pointer = SfmtStart + (index * 4);
            return pointer;
        }

        public Dictionary<string, PKM> GetPokemon()
        {
            // Party
            var party = GetParty()
                .Select((pkm, index) => new { index, pkm })
                .ToDictionary(d => $"party{d.index + 1}", d => d.pkm);

            var all = party;

            // Wild encounter
            var wildEncounter = GetSinglePkm(WildAddress);
            if (wildEncounter != null)
                all.Add("wildEncounter", wildEncounter);

            // Egg parents
            var parent1 = GetSinglePkm(Parent1Address);
            if (parent1 != null)
                all.Add("parent1", parent1);

            var parent2 = GetSinglePkm(Parent2Address);
            if (parent2 != null)
                all.Add("parent2", parent2);

            return all;
        }

        private IEnumerable<PKM> GetParty()
        {
            for (ulong i = 0; i < 6; i++)
            {
                var address = PartyAddress + i * 484;
                var data = _device.Read(address, 232);

                var pkm = PKMConverter.GetPKMfromBytes(data);

                if (pkm.ChecksumValid && pkm.Species != 0)
                    yield return pkm;
            }
        }

        private PKM GetSinglePkm(ulong address)
        {
            var data = _device.Read(address, 232);

            var pkm = PKMConverter.GetPKMfromBytes(data);

            return pkm.ChecksumValid && pkm.Species != 0
                ? pkm
                : null;
        }

        public bool EggReady()
        {
            return _device.ReadInt32(EggReadyAddress) != 0;
        }

        public IList<uint> GetEggSeeds()
        {
            var eggSeed0 = _device.ReadUInt32(EggAddress);
            var eggSeed1 = _device.ReadUInt32(EggAddress + 4);
            var eggSeed2 = _device.ReadUInt32(EggAddress + 8);
            var eggSeed3 = _device.ReadUInt32(EggAddress + 12);

            return new[] { eggSeed0, eggSeed1, eggSeed2, eggSeed3 };
        }
    }
}
