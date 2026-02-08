using System.Collections.ObjectModel;
using Cyclotron.Telemetry.DependencyInjection;
using Cyclotron.Telemetry.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Bookmarkly.App;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly ObservableCollection<int> _datas = new();
    public MainWindow()
    {
        _datas.Add(1);
        _datas.Add(1);
        _datas.Add(1);
        _datas.Add(1);
        _datas.Add(1);
        _datas.Add(1);
        _datas.Add(1);
        _datas.Add(1);
        _datas.Add(1);
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
    }
}
