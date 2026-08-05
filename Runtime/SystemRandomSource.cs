using System;

namespace CLabs.Utility {
    /// <summary>
    /// The default source: <see cref="Random"/>, seeded by the clock. Registered by any package that needs
    /// randomness and swapped for <see cref="SeededRandomSource"/> when a run has to be reproducible.
    ///
    /// It is deliberately parameterless. Buttr resolves the constructor with the most parameters and treats
    /// every parameter as required, so a single class carrying both a parameterless and a seed-taking
    /// constructor would be unconstructible — Buttr would pick the seed one and find no <c>int</c> to inject.
    /// That is why the seeded variant is its own type.
    ///
    /// Not thread-safe, because <see cref="Random"/> is not.
    /// </summary>
    public sealed class SystemRandomSource : IRandomSource {
        private readonly Random m_Random;

        public SystemRandomSource() {
            m_Random = new Random();
        }

        public float NextFloat() {
            return m_Random.NextFloat();
        }

        public int NextInt(int minInclusive, int maxExclusive) {
            return m_Random.NextInt(minInclusive, maxExclusive);
        }
    }

    /// <summary>
    /// A source pinned to a seed, so the same seed replays the same sequence. Supply it through a registration
    /// factory — <c>.WithImplementation&lt;IRandomSource&gt;(() =&gt; new SeededRandomSource(saveSeed))</c> —
    /// since the seed is the game's to choose and Buttr has no <c>int</c> to resolve.
    ///
    /// Not thread-safe, and sharing one across systems couples their sequences: consuming a value in one moves
    /// the other along. Give each system that must replay independently its own.
    /// </summary>
    public sealed class SeededRandomSource : IRandomSource {
        private readonly Random m_Random;

        public SeededRandomSource(int seed) {
            m_Random = new Random(seed);
        }

        public float NextFloat() {
            return m_Random.NextFloat();
        }

        public int NextInt(int minInclusive, int maxExclusive) {
            return m_Random.NextInt(minInclusive, maxExclusive);
        }
    }

    internal static class SystemRandomSourceInternals {
        /// <summary>
        /// The largest float below one. <see cref="Random.NextDouble"/> never returns 1.0, but a double close
        /// enough to it rounds UP to 1.0f on the cast — about once in thirty million calls — which would hand
        /// a caller a value its contract promised it would never see.
        /// </summary>
        private const float JustBelowOne = 0.99999994f;

        internal static float NextFloat(this Random random) {
            var value = (float)random.NextDouble();

            return value < 1f ? value : JustBelowOne;
        }

        internal static int NextInt(this Random random, int minInclusive, int maxExclusive) {
            if (maxExclusive <= minInclusive) {
                return minInclusive;
            }

            return random.Next(minInclusive, maxExclusive);
        }
    }
}
