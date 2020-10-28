namespace Pk3DSRNGTool.Citra
{
    using System.Collections.Generic;
    using System.Linq;
    using Magnetosphere;
    using PKHeX.Core;

    public abstract class GameState
    {
        protected readonly IDeviceRW Device;

        protected GameState(IDeviceRW device)
        {
            Device = device;
        }

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

        public abstract IList<uint> GetEggSeeds();

        public bool EggReady { get; private set; }
        public IList<uint> EggSeeds { get; private set; }

        public uint InitialSeed { get; private set; }
        public ulong CurrentSeed { get; private set; }
        public int FrameCount { get; private set; }
        public int FrameDifference { get; private set; }

        public RNGState Main { get; } = new RNGState();

        public virtual void Update()
        {
            Main.UpdateFrame();

            InitialSeed = Main.InitialSeed;
            CurrentSeed = Main.CurrentSeed;
            FrameCount = Main.FrameCount;
            FrameDifference = Main.FrameDifference;

            EggReady = Device.ReadInt32(EggReadyAddress) != 0;
            EggSeeds = GetEggSeeds();
        }

        public virtual Dictionary<string, PKM> GetPokemon()
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
    }
}