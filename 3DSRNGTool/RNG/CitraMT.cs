namespace Pk3DSRNGTool.RNG
{
    using System;

    public class CitraMT
    {
        /* Period parameters */
        private const Int32 N = 624;
        private const Int32 M = 397;
        private const uint MatrixA = 0x9908b0df; /* constant vector a */
        private const uint UpperMask = 0x80000000; /* most significant w-r bits */
        private const uint LowerMask = 0x7fffffff; /* least significant r bits */

        /* Tempering parameters */
        private static readonly uint[] Mag01 = { 0x0, MatrixA };
        private readonly uint[] _mt = new uint[N]; /* the array for the state vector  */
        private short _mti;

        public CitraMT(uint seed)
        {
            Init(seed);
        }

        public uint NextUint()
        {
            if (_mti >= 624)
                Shuffle();

            var y = _mt[_mti++];
            return y;
        }

        private void Shuffle()
        {
            short kk = 0;
            uint y;

            for (; kk < N - M; ++kk)
            {
                y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                _mt[kk] = _mt[kk + M] ^ (y >> 1) ^ Mag01[y & 0x1];
            }

            for (; kk < N - 1; ++kk)
            {
                y = (_mt[kk] & UpperMask) | (_mt[kk + 1] & LowerMask);
                _mt[kk] = _mt[kk + (M - N)] ^ (y >> 1) ^ Mag01[y & 0x1];
            }

            y = (_mt[N - 1] & UpperMask) | (_mt[0] & LowerMask);
            _mt[N - 1] = _mt[M - 1] ^ (y >> 1) ^ Mag01[y & 0x1];

            _mti = 0;
        }

        private void Init(uint seed)
        {
            _mt[0] = seed;
            for (var i = 1; i < N; i++)
                _mt[i] = (uint)(0x6C078965 * (_mt[i - 1] ^ (_mt[i - 1] >> 30)) + i);
        }
    }
}
