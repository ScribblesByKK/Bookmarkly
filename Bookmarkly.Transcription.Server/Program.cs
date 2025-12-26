using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Bookmarkly.Transcription.Server;

class Program
{
    private static ManualResetEvent? _serverExitEvent;
    private static int _refCount = 0;
    private static readonly object _lock = new object();

    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            // Handle server lifetime
            _serverExitEvent = new ManualResetEvent(false);
            
            // Register for process exit to cleanup
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Console.CancelKeyPress += OnCancelKeyPress;

            Console.WriteLine("Bookmarkly Transcription Server started.");
            
            // In a real implementation, this would register with COM using
            // Windows.ApplicationModel.Core APIs and CoRegisterClassObject.
            // For packaged apps, the system handles activation through the manifest.
            // The server just needs to stay alive while there are active instances.
            
            // Keep server alive
            _serverExitEvent.WaitOne();
            
            Console.WriteLine("Bookmarkly Transcription Server stopped.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
            return 1;
        }
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        _serverExitEvent?.Set();
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        _serverExitEvent?.Set();
    }

    // Called when an instance is created
    internal static void AddRef()
    {
        lock (_lock)
        {
            _refCount++;
        }
    }

    // Called when an instance is released
    internal static void Release()
    {
        lock (_lock)
        {
            _refCount--;
            if (_refCount <= 0)
            {
                // No more instances, signal server to exit
                _serverExitEvent?.Set();
            }
        }
    }
}
