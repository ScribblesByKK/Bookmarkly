namespace Bookmarkly.Entities.Abstractions
{
    public interface IArticleContent
    {
        string ArticleId { get; set; }

        string Content { get; set; }
    }

    public interface IUserArticleContent : IArticleContent
    {
        string UserId { get; set; }
    }
}
