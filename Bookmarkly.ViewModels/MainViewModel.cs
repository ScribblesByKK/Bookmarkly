using Bookmarkly.Entities;
using Bookmarkly.Library;
using Bookmarkly.ViewContracts;

namespace Bookmarkly.ViewModels;

public class MainViewModel
{
    private readonly BookmarkService _bookmarkService;

    public MainViewModel()
    {
        _bookmarkService = new BookmarkService();
    }
}
