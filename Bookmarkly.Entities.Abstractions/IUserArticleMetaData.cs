namespace Bookmarkly.Entities.Abstractions
{
    public interface IUserArticleMetaData : IArticleMetaData
    {
        string UserId { get; set; }

        string FolderId { get; set; }

        DateTimeOffset CreatedAt { get; set; }

        DateTimeOffset UpdatedAt { get; set; }

        bool IsRead { get; set; }

        bool IsFavorite { get; set; }
    }
}
