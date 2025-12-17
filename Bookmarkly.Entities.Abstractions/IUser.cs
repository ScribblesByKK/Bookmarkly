namespace Bookmarkly.Entities.Abstractions
{
    public interface IUser
    {
        string Id { get; set; }

        string DisplayName { get; set; }
    }
}
