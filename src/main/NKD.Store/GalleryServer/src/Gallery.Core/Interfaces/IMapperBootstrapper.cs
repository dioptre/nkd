namespace Gallery.Core.Interfaces
{
    public interface IMapperBootstrapper
    {
        void RegisterMappings();
        void AssertConfigurationIsValid();
    }
}