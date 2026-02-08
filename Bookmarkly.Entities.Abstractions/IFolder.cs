namespace Bookmarkly.Entities.Abstractions;

public interface IFolder
{
    string Id { get; set; }

    string UserId { get; set; }

    string Name { get; set; }

    string ParentFolderId { get; set; }
}
