namespace Instapaper.Entities;

public class UserArticleMetaData : ArticleMetaData, IUserArticleMetaData
{
    public string UserId { get; set; } = default!;

    public string FolderId { get; set; } = default!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public bool IsRead { get; set; } = true;

    public bool IsFavorite { get; set; } = false;
}
