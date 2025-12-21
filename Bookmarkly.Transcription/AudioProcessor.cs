using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bookmarkly.Transcription;

/// <summary>
/// Audio processor that converts audio files to mel spectrograms for Whisper
/// </summary>
public class AudioProcessor
{
    private const int SampleRate = 16000;
    private const int MelBins = 80;
    private const int WindowSize = 400; // 25ms at 16kHz
    private const int HopSize = 160;    // 10ms at 16kHz
    private const int ChunkSamples = SampleRate * 30; // 30 seconds

    /// <summary>
    /// Loads and resamples audio file to 16kHz mono
    /// </summary>
    public async Task<float[]> LoadAudioAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var reader = new AudioFileReader(filePath);
                
                // Resample to 16kHz mono if needed
                var resampler = new MediaFoundationResampler(reader, new WaveFormat(SampleRate, 1))
                {
                    ResamplerQuality = 60
                };

                var samples = new List<float>();
                var buffer = new float[8192];
                int read;
                
                while ((read = resampler.Read(buffer, 0, buffer.Length)) > 0)
                {
                    samples.AddRange(buffer.Take(read));
                }

                resampler.Dispose();
                return samples.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load audio file: {ex.Message}", ex);
            }
        });
    }

    /// <summary>
    /// Converts audio samples to mel spectrogram
    /// </summary>
    public float[,] ComputeMelSpectrogram(float[] samples)
    {
        // This is a simplified version
        // Production implementation would use proper STFT and mel filterbank
        
        int numFrames = (samples.Length - WindowSize) / HopSize + 1;
        var melSpectrogram = new float[MelBins, numFrames];
        
        // Simplified: just compute basic spectral features
        for (int frame = 0; frame < numFrames; frame++)
        {
            int start = frame * HopSize;
            var window = samples.Skip(start).Take(WindowSize).ToArray();
            
            // Apply Hann window
            for (int i = 0; i < window.Length; i++)
            {
                window[i] *= 0.5f * (1 - MathF.Cos(2 * MathF.PI * i / (window.Length - 1)));
            }
            
            // Simplified mel computation - in production would use FFT + mel filterbank
            for (int mel = 0; mel < MelBins; mel++)
            {
                float energy = 0;
                int binSize = window.Length / MelBins;
                for (int i = 0; i < binSize; i++)
                {
                    int idx = mel * binSize + i;
                    if (idx < window.Length)
                    {
                        energy += window[idx] * window[idx];
                    }
                }
                melSpectrogram[mel, frame] = MathF.Log(MathF.Max(energy, 1e-10f));
            }
        }
        
        return melSpectrogram;
    }

    /// <summary>
    /// Splits audio into 30-second chunks for processing
    /// </summary>
    public List<float[]> SplitIntoChunks(float[] samples)
    {
        var chunks = new List<float[]>();
        
        for (int i = 0; i < samples.Length; i += ChunkSamples)
        {
            int chunkSize = Math.Min(ChunkSamples, samples.Length - i);
            var chunk = new float[chunkSize];
            Array.Copy(samples, i, chunk, 0, chunkSize);
            chunks.Add(chunk);
        }
        
        return chunks;
    }
}
