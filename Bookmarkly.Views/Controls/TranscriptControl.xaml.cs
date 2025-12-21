using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Bookmarkly.Transcription;

namespace Bookmarkly.Views.Controls;

public sealed partial class TranscriptControl : UserControl, INotifyPropertyChanged
{
    private StorageFile? _audioFile;
    private string _transcript = "No transcript available. Please select an audio file.";
    private bool _isTranscribing;
    private string _selectedLanguage = "en";
    private string _fileDisplayText = "Drop audio file here or click Browse";

    public TranscriptControl()
    {
        InitializeComponent();
        LoadLanguagesAsync();
    }

    #region Dependency Properties

    public static readonly DependencyProperty AudioFileProperty =
        DependencyProperty.Register(
            nameof(AudioFile),
            typeof(StorageFile),
            typeof(TranscriptControl),
            new PropertyMetadata(null, OnAudioFileChanged));

    public StorageFile? AudioFile
    {
        get => (StorageFile?)GetValue(AudioFileProperty);
        set => SetValue(AudioFileProperty, value);
    }

    public static readonly DependencyProperty TranscriptProperty =
        DependencyProperty.Register(
            nameof(Transcript),
            typeof(string),
            typeof(TranscriptControl),
            new PropertyMetadata(string.Empty));

    public string Transcript
    {
        get => _transcript;
        private set
        {
            if (_transcript != value)
            {
                _transcript = value;
                SetValue(TranscriptProperty, value);
                OnPropertyChanged();
            }
        }
    }

    public static readonly DependencyProperty IsTranscribingProperty =
        DependencyProperty.Register(
            nameof(IsTranscribing),
            typeof(bool),
            typeof(TranscriptControl),
            new PropertyMetadata(false));

    public bool IsTranscribing
    {
        get => _isTranscribing;
        private set
        {
            if (_isTranscribing != value)
            {
                _isTranscribing = value;
                SetValue(IsTranscribingProperty, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotTranscribing));
                OnPropertyChanged(nameof(HasTranscript));

                // Control shimmer animation
                if (value)
                {
                    ShimmerStoryboard.Begin();
                }
                else
                {
                    ShimmerStoryboard.Stop();
                }
            }
        }
    }

    public bool IsNotTranscribing => !IsTranscribing;

    public bool HasTranscript => !IsTranscribing && !string.IsNullOrEmpty(Transcript) 
        && Transcript != "No transcript available. Please select an audio file.";

    public static readonly DependencyProperty SelectedLanguageProperty =
        DependencyProperty.Register(
            nameof(SelectedLanguage),
            typeof(string),
            typeof(TranscriptControl),
            new PropertyMetadata("en"));

    public string SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (_selectedLanguage != value)
            {
                _selectedLanguage = value;
                SetValue(SelectedLanguageProperty, value);
                OnPropertyChanged();
            }
        }
    }

    public string FileDisplayText
    {
        get => _fileDisplayText;
        set
        {
            if (_fileDisplayText != value)
            {
                _fileDisplayText = value;
                OnPropertyChanged();
            }
        }
    }

    #endregion

    #region Event Handlers

    private static void OnAudioFileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TranscriptControl control && e.NewValue is StorageFile file)
        {
            control._audioFile = file;
            control.FileDisplayText = file.Name;
            _ = control.TranscribeAudioAsync(file);
        }
    }

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker();
            
            // Get the window handle for the picker
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".wav");
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".m4a");
            picker.FileTypeFilter.Add(".flac");
            picker.FileTypeFilter.Add(".ogg");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                AudioFile = file;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to open file picker: {ex.Message}");
        }
    }

    private async void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(Transcript))
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(Transcript);
                Clipboard.SetContent(dataPackage);

                // Show brief feedback
                CopyButton.Content = "Copied!";
                await Task.Delay(2000);
                
                // Restore button
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                panel.Children.Add(new FontIcon { Glyph = "\uE8C8", FontSize = 16 });
                panel.Children.Add(new TextBlock { Text = "Copy" });
                CopyButton.Content = panel;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to copy to clipboard: {ex.Message}");
        }
    }

    private void FileArea_DragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        
        if (e.DragUIOverride != null)
        {
            e.DragUIOverride.Caption = "Drop audio file";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
        }
    }

    private async void FileArea_Drop(object sender, DragEventArgs e)
    {
        try
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0 && items[0] is StorageFile file)
                {
                    // Validate file type
                    var validExtensions = new[] { ".wav", ".mp3", ".m4a", ".flac", ".ogg" };
                    if (validExtensions.Contains(file.FileType.ToLower()))
                    {
                        AudioFile = file;
                    }
                    else
                    {
                        ShowError("Unsupported file format. Please use WAV, MP3, M4A, FLAC, or OGG.");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to process dropped file: {ex.Message}");
        }
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is ComboBoxItem item && item.Tag is string langCode)
        {
            SelectedLanguage = langCode;
            
            // Re-transcribe if we have a file
            if (_audioFile != null)
            {
                _ = TranscribeAudioAsync(_audioFile);
            }
        }
    }

    #endregion

    #region Transcription

    private async Task LoadLanguagesAsync()
    {
        try
        {
            // Try to get languages from service
            var service = new TranscriptionService();
            var languages = await service.GetSupportedLanguagesAsync();

            // Map languages to codes
            var languageMap = new[]
            {
                ("Auto-detect", "en"),
                ("English (en)", "en"),
                ("Spanish (es)", "es"),
                ("French (fr)", "fr"),
                ("German (de)", "de"),
                ("Italian (it)", "it"),
                ("Portuguese (pt)", "pt"),
                ("Dutch (nl)", "nl"),
                ("Russian (ru)", "ru"),
                ("Chinese (zh)", "zh"),
                ("Japanese (ja)", "ja"),
                ("Korean (ko)", "ko"),
                ("Arabic (ar)", "ar"),
                ("Hindi (hi)", "hi")
            };

            foreach (var (display, code) in languageMap)
            {
                LanguageComboBox.Items.Add(new ComboBoxItem
                {
                    Content = display,
                    Tag = code
                });
            }

            LanguageComboBox.SelectedIndex = 0;
        }
        catch
        {
            // Fallback to basic list
            LanguageComboBox.Items.Add(new ComboBoxItem { Content = "Auto-detect", Tag = "en" });
            LanguageComboBox.Items.Add(new ComboBoxItem { Content = "English", Tag = "en" });
            LanguageComboBox.SelectedIndex = 0;
        }
    }

    private async Task TranscribeAudioAsync(StorageFile file)
    {
        IsTranscribing = true;
        ErrorInfoBar.IsOpen = false;
        Transcript = string.Empty;

        try
        {
            // Connect to transcription service
            var service = new TranscriptionService();
            
            // Transcribe with selected language
            var result = await service.TranscribeWithLanguageAsync(file, SelectedLanguage);
            
            Transcript = result;
        }
        catch (Exception ex)
        {
            ShowError($"Transcription failed: {ex.Message}");
            Transcript = "Transcription failed. Please try again.";
        }
        finally
        {
            IsTranscribing = false;
        }
    }

    #endregion

    #region Helpers

    private void ShowError(string message)
    {
        ErrorInfoBar.Message = message;
        ErrorInfoBar.IsOpen = true;
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
