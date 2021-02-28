namespace Pk3DSRNGTool.Citra
{
    using System.Collections.Generic;
    using PKHeX.Core;

    public interface IManager
    {
        void UpdateFrame();

        Dictionary<string, PKM> GetPokemon();
        bool EggReady();

        IList<uint> GetEggSeeds();

        IList<uint> GetTinyMT { get; }

        uint InitialSeed { get; }
        ulong CurrentSeed { get; }
        int FrameCount { get; }
        int FrameDifference { get; }

        uint GetSaveVariable { get; }
        uint GetTimeVariable { get; }
    }
}
