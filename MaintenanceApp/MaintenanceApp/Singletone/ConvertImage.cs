using System;
using System.IO;
using System.Reflection;
using Microsoft.Maui.Graphics;

namespace MaintenanceApp.Singletone
{
    public class ConvertImage
    {
        private static readonly Lazy<ConvertImage> lazy = new Lazy<ConvertImage>(() => new ConvertImage());
        public static ConvertImage Instance => lazy.Value;

        public static async Task<byte[]> ImageSourceToByteArrayAsync(ImageSource imageSource)
        {

            var assem = Assembly.GetExecutingAssembly();
            using var stream = assem.GetManifestResourceStream("ProjectName.Resources.Images.test.png");
            byte[] bytesAvailable = new byte[stream.Length];
            stream.Read(bytesAvailable, 0, bytesAvailable.Length);
            return bytesAvailable;
        }
    }
}
