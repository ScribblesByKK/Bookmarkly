namespace Instapaper.Entities;

public class ArticleContent : IArticleContent
{
    public string ArticleId { get; set; } = default!;

    public string Content { get; set; } = default!;
}
