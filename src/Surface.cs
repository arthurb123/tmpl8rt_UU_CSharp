using System.Runtime.InteropServices;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

using Image = SixLabors.ImageSharp.Image;

namespace tmpl8rt_UU_CSharp {
    public class Surface : IDisposable {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public unsafe float* PixelsPtr { get; private set; }

        public unsafe Surface(string fileName) {
            try
            {
                using (Image<Rgba32> image = Image.Load<Rgba32>(fileName))
                {
                    Width = image.Width;
                    Height = image.Height;
                    PixelsPtr = AllocateBuffer();

                    image.ProcessPixelRows(processPixels: (accessor) =>
                    {
                        // Iterate over the pixels to get the RGB values
                        for (int y = 0; y < Height; y++) {
                            float* offset = PixelsPtr + (y * Width * 4);
                            for (int x = 0; x < Width; x++)
                            {
                                Span<Rgba32> row = accessor.GetRowSpan(y);
                                Rgba32 pixelColor = row[x];
                                
                                float* pixel = offset + x * 4;
                                pixel[0] = pixelColor.R / 255f;
                                pixel[1] = pixelColor.G / 255f;
                                pixel[2] = pixelColor.B / 255f;
                                pixel[3] = pixelColor.A / 255f;
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not load surface image {ex}");
            }
        }

        public unsafe Vector4D<float> GetAlbedo(float u, float v) {
            // Repeat wrapping
            u -= (float)Math.Floor(u);
            v -= (float)Math.Floor(v);

            // Convert to pixel coordinates
            int x = (int)(u * Width) % Width;
            int y = (int)(v * Height) % Height;

            // Calculate offset
            float* pixel = PixelsPtr + (y * Width + x) * 4;

            // Return color
            return new Vector4D<float>(
                pixel[0],
                pixel[1],
                pixel[2],
                pixel[3]
            );
        }

        public unsafe void Dispose()
        {
            Marshal.FreeHGlobal((IntPtr)PixelsPtr);
        }

        private unsafe float* AllocateBuffer() {
            uint size = (uint)(Width * Height * 4 * sizeof(float));
            return (float*)Marshal.AllocHGlobal((int)size).ToPointer();
        }
    }
}