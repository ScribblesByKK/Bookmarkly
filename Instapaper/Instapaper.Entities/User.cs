namespace Instapaper.Entities;

public class User : IUser
{
    public string Id { get; set; } = default!;

    public string Username { get; set; } = default!;

    public string DisplayName { get; set; } = default!;
}
