using Bookmarkly.Entities;
using System.Collections.Generic;

namespace Bookmarkly.ViewContracts;

public interface IBookmarkService
{
    IEnumerable<Bookmark> GetBookmarks();
}
