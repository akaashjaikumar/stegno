using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;

namespace stegno
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string imagePath = @"C:\Users\akaas\OneDrive\Pictures\pxfuel (1).jpg";

            // Load the image
            Bitmap bitmap = new Bitmap(imagePath);

            // Create MemoryStreams to hold the combined audio data
            MemoryStream combinedStream = new MemoryStream();

            // Audio parameters
            int sampleRate = 44100; // 44.1kHz
            int durationSeconds = bitmap.Width * bitmap.Height; // Adjust duration based on image size
            int amplitude = 32760; // Adjust amplitude for volume

            // Iterate over each pixel
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    // Get the color of the current pixel
                    Color pixelColor = bitmap.GetPixel(x, y);

                    // Convert RGB values to sample values (-32768 to 32767)
                    short redSample = (short)(pixelColor.R * (amplitude / 255));
                    short greenSample = (short)(pixelColor.G * (amplitude / 255));
                    short blueSample = (short)(pixelColor.B * (amplitude / 255));

                    // Write the sample values to the combined memory stream
                    byte[] redBytes = BitConverter.GetBytes(redSample);
                    byte[] greenBytes = BitConverter.GetBytes(greenSample);
                    byte[] blueBytes = BitConverter.GetBytes(blueSample);

                    combinedStream.Write(redBytes, 0, 2);
                    combinedStream.Write(greenBytes, 0, 2);
                    combinedStream.Write(blueBytes, 0, 2);
                }
            }

            // Convert combined memory stream to byte array
            byte[] combinedAudioData = combinedStream.ToArray();

            // Create a new WaveFormat
            WaveFormat waveFormat = new WaveFormat(sampleRate, 16, 1);

            // Write the combined audio data to a single audio file
            WriteAudioFile(combinedAudioData, waveFormat, @"C:\Users\akaas\OneDrive\Pictures\combined.wav");

            // Dispose of resources
            bitmap.Dispose();
            combinedStream.Dispose();

            Console.WriteLine("Combined audio file saved successfully.");
        }

        private static void WriteAudioFile(byte[] audioData, WaveFormat waveFormat, string filePath)
        {
            using (WaveFileWriter waveFileWriter = new WaveFileWriter(filePath, waveFormat))
            {
                waveFileWriter.Write(audioData, 0, audioData.Length);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Load the combined audio file
            string audioFilePath = @"C:\Users\akaas\OneDrive\Pictures\combined.wav";

            // Read the audio file
            byte[] audioData;
            using (WaveFileReader waveFileReader = new WaveFileReader(audioFilePath))
            {
                audioData = new byte[waveFileReader.Length];
                waveFileReader.Read(audioData, 0, (int)waveFileReader.Length);
            }

            // Audio parameters
            int sampleRate = 44100; // 44.1kHz
            int amplitude = 32760; // Adjust amplitude for volume
            int bytesPerSample = 2; // 16-bit audio (2 bytes per sample)

            // Calculate the number of pixels
            int pixelCount = audioData.Length / (3 * bytesPerSample);

            // Calculate the image dimensions
            int width = (int)Math.Sqrt(pixelCount);
            int height = pixelCount / width;

            // Create a new bitmap to store the reconstructed image
            Bitmap reconstructedBitmap = new Bitmap(width, height);

            // Iterate over each pixel
            int dataIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Read the red, green, and blue samples for the current pixel
                    short redSample = BitConverter.ToInt16(audioData, dataIndex);
                    short greenSample = BitConverter.ToInt16(audioData, dataIndex + bytesPerSample);
                    short blueSample = BitConverter.ToInt16(audioData, dataIndex + 2 * bytesPerSample);

                    // Normalize the sample values to the range [0, 255]
                    int red = (int)Math.Round((double)redSample / amplitude * 255);
                    int green = (int)Math.Round((double)greenSample / amplitude * 255);
                    int blue = (int)Math.Round((double)blueSample / amplitude * 255);

                    // Clamp the color values to ensure they are within the valid range
                    red = Math.Max(0, Math.Min(255, red));
                    green = Math.Max(0, Math.Min(255, green));
                    blue = Math.Max(0, Math.Min(255, blue));

                    // Set the color of the current pixel in the reconstructed image
                    reconstructedBitmap.SetPixel(x, y, Color.FromArgb(red, green, blue));

                    // Move to the next pixel in the audio data
                    dataIndex += 3 * bytesPerSample;
                }
            }

            // Save the reconstructed image in the same directory
            string reconstructedImagePath = Path.Combine(Path.GetDirectoryName(audioFilePath), "reconstructed_image.jpg");
            reconstructedBitmap.Save(reconstructedImagePath);

            // Display the reconstructed image in the picture box
            pictureBox1.Image = reconstructedBitmap;

            // Display a message to indicate that the image has been saved
            MessageBox.Show("Reconstructed image saved successfully.");
        }

    }
}
