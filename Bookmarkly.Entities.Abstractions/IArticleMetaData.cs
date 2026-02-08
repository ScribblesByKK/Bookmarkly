namespace Bookmarkly.Entities.Abstractions;

public interface IArticleMetaData
{
    string Id { get; set; }

    string Title { get; set; }

    string Url { get; set; }

    string Summary { get; set; }

    string ThumbnailUrl { get; set; }
}
