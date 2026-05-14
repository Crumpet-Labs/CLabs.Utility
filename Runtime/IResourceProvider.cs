namespace CLabs.Utility
{
    public interface IResourceProvider
    {
        bool CanHandle(IDefinition resource);
        bool HasResource(IDefinition resource, int quantity);
        void Consume(IDefinition resource, int quantity);
        void Grant(IDefinition resource, int quantity);
    }
}
