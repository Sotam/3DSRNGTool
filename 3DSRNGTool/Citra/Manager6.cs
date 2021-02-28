namespace Pk3DSRNGTool.Citra
{
    using System.Collections.Generic;
    using System.Linq;
    using Exceptions;
    using Magnetosphere;
    using PKHeX.Core;
    using RNG;

    public abstract class Manager6 : IManager
    {
        public readonly IDeviceRW Device;
        private CitraMT MT { get; set; }

        public abstract ulong PartyAddress { get; }
        public abstract ulong WildAddress { get; }

        public abstract ulong SeedAddress { get; }

        public abstract ulong MtStart { get; }
        public abstract ulong MtIndex { get; }

        public abstract ulong TinyStart { get; }
        public abstract uint SaveVariable { get; }


        public abstract ulong EggReadyAddress { get; }
        public abstract ulong EggAddress { get; }
        public abstract ulong Parent1Address { get; }
        public abstract ulong Parent2Address { get; }

        public uint InitialSeed { get; private set; }
        public ulong CurrentSeed { get; private set; }
        public int FrameCount { get; private set; } = -1;
        public int FrameDifference { get; private set; } = -1;

        public uint GetSaveVariable { get; private set; }
        public uint GetTimeVariable { get; private set; }

        public IList<uint> GetTinyMT { get; private set; }

        protected Manager6(IDeviceRW device)
        {
            Device = device;

            InitialSeed = Device.ReadUInt32(SeedAddress);
            MT = new CitraMT(InitialSeed);
            CurrentSeed = InitialSeed;
        }

        public void UpdateFrame()
        {
            try
            {
                if (Device.ReadUInt32(SeedAddress) != InitialSeed)
                {
                    InitialSeed = Device.ReadUInt32(SeedAddress);
                    MT = new CitraMT(InitialSeed);
                    CurrentSeed = InitialSeed;
                    FrameCount = -1;
                    FrameDifference = -1;
                }

                var game = CalcCurrentSeed();
                var seed = CurrentSeed;

                var frameCount = 0;
                var frameDifference = FrameCount;

                while (game != seed)
                {
                    seed = MT.NextUint();
                    frameCount++;

                    if (frameCount > 5000000)
                        throw new FrameOutOfRangeException();
                }

                FrameCount += frameCount;

                var difference = FrameCount - frameDifference;
                if (difference != 0)
                    FrameDifference = difference;

                CurrentSeed = seed;

                var tiny0 = Device.ReadUInt32(TinyStart);
                var tiny1 = Device.ReadUInt32(TinyStart + 4);
                var tiny2 = Device.ReadUInt32(TinyStart + 8);
                var tiny3 = Device.ReadUInt32(TinyStart + 12);

                GetTinyMT = new List<uint> { tiny0, tiny1, tiny2, tiny3 };

                GetSaveVariable = Device.ReadUInt32(SaveVariable);
                GetTimeVariable = (InitialSeed - GetSaveVariable) & 0xffffffff; // (initial_seed - save_variable) & 0xffffffff
            }
            catch (FrameOutOfRangeException)
            {
                MT = new CitraMT(InitialSeed);
                FrameCount = -1;
                FrameDifference = -1;
            }
        }

        private uint CalcCurrentSeed()
        {
            var index = Device.ReadUInt32(MtIndex);
            var pointer = GetMTPointer(index);
            var seed = Device.ReadUInt32(pointer);

            return seed;
        }

        private ulong GetMTPointer(uint index)
        {
            ulong pointer;
            if (index == 624)
                pointer = MtStart;
            else
                pointer = MtStart + (index * 4);
            return pointer;
        }

        public virtual Dictionary<string, PKM> GetPokemon()
        {
            // Party
            var party = GetParty()
                .Select((pkm, index) => new { index, pkm })
                .ToDictionary(d => $"party{d.index + 1}", d => d.pkm);

            var all = party;

            // TODO See ManagerXY.cs
            // Wild encounter
            //var wildEncounter = GetSinglePkm(WildAddress);
            //if (wildEncounter != null)
            //    all.Add("wildEncounter", wildEncounter);

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
                var data = Device.Read(address, 232);

                var pkm = PKMConverter.GetPKMfromBytes(data);

                if (pkm.ChecksumValid && pkm.Species != 0)
                    yield return pkm;
            }
        }

        private PKM GetSinglePkm(ulong address)
        {
            var data = Device.Read(address, 232);

            var pkm = PKMConverter.GetPKMfromBytes(data);

            return pkm.ChecksumValid && pkm.Species != 0
                ? pkm
                : null;
        }

        public bool EggReady()
        {
            return Device.ReadInt32(EggReadyAddress) != 0;
        }

        public IList<uint> GetEggSeeds()
        {
            var eggSeed0 = Device.ReadUInt32(EggAddress);
            var eggSeed1 = Device.ReadUInt32(EggAddress + 4);

            return new[] { eggSeed0, eggSeed1 };
        }
    }
}