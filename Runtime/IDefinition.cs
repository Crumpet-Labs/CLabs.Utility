namespace CLabs.Utility
{
    /// <summary>
    /// Marker interface for engine-agnostic definition types.
    /// Implemented by ScriptableObject-based definitions in Unity adapters.
    /// </summary>
    public interface IDefinition {
        string Name { get; }
    }
}
