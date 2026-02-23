using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Bookmarkly.App;

/// <summary>
/// Sign-in page that allows users to authenticate with FreshRSS or Instapaper.
/// </summary>
public sealed partial class SignInPage : Page
{
    public SignInPage()
    {
        InitializeComponent();
    }

    private void OnFreshRssSignInClick(object sender, RoutedEventArgs e)
    {
        // TODO: Implement FreshRSS sign-in flow
    }

    private void OnInstapaperSignInClick(object sender, RoutedEventArgs e)
    {
        // TODO: Implement Instapaper sign-in flow
    }
}
