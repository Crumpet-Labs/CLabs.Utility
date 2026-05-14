namespace CLabs.Utility
{
    public sealed class CompositeResourceProvider : IResourceProvider
    {
        private readonly IResourceProvider[] m_Providers;

        public CompositeResourceProvider(params IResourceProvider[] providers)
        {
            m_Providers = providers;
        }

        public bool CanHandle(IDefinition resource)
        {
            foreach (var provider in m_Providers)
                if (provider.CanHandle(resource)) return true;
            return false;
        }

        public bool HasResource(IDefinition resource, int quantity)
        {
            foreach (var provider in m_Providers)
                if (provider.CanHandle(resource))
                    return provider.HasResource(resource, quantity);
            return false;
        }

        public void Consume(IDefinition resource, int quantity)
        {
            foreach (var provider in m_Providers)
            {
                if (provider.CanHandle(resource))
                {
                    provider.Consume(resource, quantity);
                    return;
                }
            }
        }

        public void Grant(IDefinition resource, int quantity)
        {
            foreach (var provider in m_Providers)
            {
                if (provider.CanHandle(resource))
                {
                    provider.Grant(resource, quantity);
                    return;
                }
            }
        }
    }
}
