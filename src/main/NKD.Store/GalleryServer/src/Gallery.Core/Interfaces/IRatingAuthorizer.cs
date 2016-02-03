namespace Gallery.Core.Interfaces
{
    public interface IRatingAuthorizer
    {
        void ValidateNonce(string nonce);
    }
}