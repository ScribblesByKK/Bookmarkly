using Bookmarkly.Entities;
using System.Collections.Generic;

namespace Bookmarkly.Library;

public class BookmarkService
{
    public IEnumerable<Bookmark> GetBookmarks()
    {
        return new List<Bookmark>();
    }
}
