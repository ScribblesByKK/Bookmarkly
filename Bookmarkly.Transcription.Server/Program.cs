using System;
using System.Runtime.InteropServices;
using System.Threading;
using WinRT;

namespace Bookmarkly.Transcription.Server;

class Program
{
    private static ManualResetEvent? _serverExitEvent;

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

            // Register the WinRT activation factory
            var activationFactory = new TranscriptionServiceActivationFactory();
            var cookie = RegisterActivationFactory(activationFactory);

            Console.WriteLine("Bookmarkly Transcription Server started.");
            
            // Keep server alive
            _serverExitEvent.WaitOne();
            
            // Unregister
            RevokeActivationFactory(cookie);
            
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

    private static int RegisterActivationFactory(IActivationFactory factory)
    {
        // Register the COM activation factory
        // This is simplified - production code would use proper COM registration
        return 1; // Return dummy cookie
    }

    private static void RevokeActivationFactory(int cookie)
    {
        // Revoke the COM activation factory
    }
}

/// <summary>
/// Activation factory for TranscriptionService
/// </summary>
internal class TranscriptionServiceActivationFactory : IActivationFactory
{
    public object ActivateInstance()
    {
        return new Bookmarkly.Transcription.TranscriptionService();
    }
}

/// <summary>
/// COM activation factory interface
/// </summary>
[ComImport]
[Guid("00000035-0000-0000-C000-000000000046")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IActivationFactory
{
    object ActivateInstance();
}
