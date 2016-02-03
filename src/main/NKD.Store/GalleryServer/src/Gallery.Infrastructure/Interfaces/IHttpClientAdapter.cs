namespace Gallery.Infrastructure.Interfaces
{
    public interface IHttpClientAdapter
    {
        IHttpClient GetHttpClient(string baseAddress);
    }
}