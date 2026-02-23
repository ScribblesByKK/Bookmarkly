using Cyclotron.Extensions.DependencyInjection;
using Cyclotron.Telemetry.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;

namespace Bookmarkly.App;

/// <summary>
/// Main application window that hosts page navigation.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        var services = new ServiceCollection();

        services.AddCyclotronTelemetry(options =>
        {
            options.ServiceName = "Bookmarkly";
            options.ServiceVersion = "1.0.0";
            options.DefaultModule = "main";

            options.Logging.MinimumLevel = LogLevel.Debug;
            options.Logging.File.Path = "{LocalAppData}/Bookmarkly/logs/app-.log";
            options.Logging.File.RetainedFileCountLimit = 3; // Already set as default
            options.Logging.File.RollingInterval = Serilog.RollingInterval.Day;
        });

        var provider = services.BuildServiceProvider();
        var logger = provider.GetRequiredService<ICyclotronLogger>().ForModule("Instapaper");

        logger.LogInformation("Starting operation"); // Caller info auto-captured!

        RootFrame.Navigate(typeof(SignInPage));
    }
}
