namespace CLabs.Utility {
    /// <summary>
    /// Where a package gets its randomness, so that "random" is a dependency rather than a hidden static.
    ///
    /// A package that reaches for <c>new System.Random()</c> can never be tested for distribution, can never be
    /// replayed, and shares one unsynchronised instance across every caller. Taking the source by injection
    /// answers all three at once: a test supplies a scripted sequence, a game supplies its save seed, and a
    /// consumer that rolls off the main thread supplies its own instance rather than racing on a shared one.
    ///
    /// Implementations are NOT required to be thread-safe. <see cref="SystemRandomSource"/> is not.
    /// </summary>
    public interface IRandomSource {
        /// <summary>A value in <c>[0, 1)</c>. One is never returned, so it is safe to scale by a total weight.</summary>
        float NextFloat();

        /// <summary>
        /// A value in <c>[minInclusive, maxExclusive)</c>. An empty or inverted range returns
        /// <paramref name="minInclusive"/> rather than throwing, because callers derive these bounds from authored
        /// data, where a min above a max is a content mistake and not worth an exception.
        /// </summary>
        int NextInt(int minInclusive, int maxExclusive);
    }
}
