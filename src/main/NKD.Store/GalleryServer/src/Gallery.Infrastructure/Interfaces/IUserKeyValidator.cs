namespace Gallery.Infrastructure.Interfaces
{
    public interface IUserKeyValidator
    {
        bool IsValidUserKey(string key);
    }
}