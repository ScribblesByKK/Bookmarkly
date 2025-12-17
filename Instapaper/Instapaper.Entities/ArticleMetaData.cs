namespace Instapaper.Entities
{
    public class ArticleMetaData : IArticleMetaData
    {
        public string Id { get; set; } = default!;

        public string Title { get; set; } = default!;

        public string Url { get; set; } = default!;

        public string Summary { get; set; } = default!;

        public string ThumbnailUrl { get; set; } = default!;
    }
}
