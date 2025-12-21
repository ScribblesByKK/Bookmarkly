# Bookmarkly Transcription Service

This directory contains the transcription service implementation for Bookmarkly, featuring an out-of-process COM server architecture using Whisper AI models.

## Architecture

### Projects

#### Bookmarkly.Transcription.Abstractions
WinRT Contract Library that defines the `ITranscriptionService` interface. This project produces a `.winmd` file that both the server and client reference.

**Interface Methods:**
- `TranscribeAsync(StorageFile audioFile)` - Transcribes audio file with auto-detected language
- `TranscribeWithLanguageAsync(StorageFile audioFile, string languageCode)` - Transcribes with specified language
- `GetSupportedLanguagesAsync()` - Returns list of supported languages

#### Bookmarkly.Transcription
Class library containing the actual ONNX/Whisper transcription logic.

**Key Classes:**
- `WhisperTranscriber` - Loads ONNX models and performs inference
- `AudioProcessor` - Converts audio to mel spectrogram (80 mel bins, 16kHz sample rate)
- `WhisperTokenizer` - Decodes token IDs to text
- `TranscriptionService` - WinRT implementation of ITranscriptionService

**Dependencies:**
- Microsoft.ML.OnnxRuntime (1.16.3) - CPU inference
- Microsoft.ML.OnnxRuntime.DirectML (1.16.3) - GPU acceleration
- NAudio (2.2.1) - Audio processing

#### Bookmarkly.Transcription.Server
Out-of-process WinRT COM Server executable that hosts the transcription service.

**Features:**
- Single instance server
- Automatic lifetime management
- Graceful shutdown handling
- Registered via Package.appxmanifest for packaged activation

## TranscriptControl UserControl

Located in `Bookmarkly.Views/Controls/TranscriptControl.xaml`

### Features

1. **File Input**
   - Drag-and-drop support for audio files
   - File picker for browsing
   - Supported formats: .wav, .mp3, .m4a, .flac, .ogg

2. **Shimmer Loading Effect**
   - Animated gradient shimmer while transcription is in progress
   - Shows 5 shimmer lines of varying widths

3. **Language Picker**
   - Top-right dropdown with supported languages
   - Defaults to "Auto-detect"
   - Supports 13+ languages including English, Spanish, French, German, Chinese, Japanese, etc.

4. **Copy Button**
   - Top-right button with copy icon
   - Copies transcript to clipboard
   - Shows "Copied!" feedback
   - Disabled when no transcript available

5. **Transcript Display**
   - Scrollable text area
   - Selectable text
   - Proper text wrapping

### Dependency Properties

- `AudioFile` (StorageFile) - The audio file to transcribe
- `Transcript` (string) - The resulting transcript text (read-only)
- `IsTranscribing` (bool) - Indicates loading state
- `SelectedLanguage` (string) - The selected language code

### Usage Example

```xaml
<controls:TranscriptControl 
    x:Name="transcriptControl"
    AudioFile="{x:Bind ViewModel.SelectedAudioFile, Mode=OneWay}" />
```

## Whisper Model Integration

The service is designed to use Whisper base models from HuggingFace:
- Encoder: https://huggingface.co/onnx-community/whisper-base/resolve/main/onnx/encoder_model.onnx
- Decoder: https://huggingface.co/onnx-community/whisper-base/resolve/main/onnx/decoder_model_merged.onnx

### Audio Processing

1. Audio is resampled to 16kHz mono
2. Converted to mel spectrogram:
   - 80 mel frequency bins
   - 400 sample window (25ms)
   - 160 sample hop (10ms)
   - Hann window
3. For files longer than 30 seconds, audio is split into chunks and processed sequentially

### Placeholder Mode

When ONNX models are not available, the service operates in "placeholder mode" and returns demonstration transcripts. This allows the UI and architecture to be tested without requiring large model files.

## Package Manifest Configuration

The out-of-process server is registered in `Bookmarkly.App/Package.appxmanifest`:

```xml
<Extensions>
  <uap5:Extension Category="windows.activatableClass.outOfProcessServer">
    <uap5:OutOfProcessServer ServerName="Bookmarkly.Transcription.Server"
                             uap5:IdentityType="activateAsPackage"
                             uap5:RunFullTrust="true">
      <uap5:Path>Bookmarkly.Transcription.Server\Bookmarkly.Transcription.Server.exe</uap5:Path>
      <uap5:Instancing>singleInstance</uap5:Instancing>
      <uap5:ActivatableClass ActivatableClassId="Bookmarkly.Transcription.TranscriptionService" />
    </uap5:OutOfProcessServer>
  </uap5:Extension>
</Extensions>
```

## Building

The projects require:
- .NET 10.0
- Windows SDK 10.0.26100.0+
- Windows App SDK 1.8.251003001+

Build with MSBuild or Visual Studio on Windows:
```bash
msbuild Bookmarkly.slnx /p:Configuration=Release /p:Platform=x64
```

## Error Handling

The service handles various error conditions:
- File not found
- Unsupported audio format
- Model loading failure
- Out of memory
- Transcription failures

All errors are caught and user-friendly messages are displayed in the TranscriptControl UI.
