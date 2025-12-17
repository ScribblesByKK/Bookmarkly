namespace Instapaper.Entities
{
    public class Folder : IFolder
    {
        public string Id { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ParentFolderId { get; set; } = default!;
    }
}
