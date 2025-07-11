using System;
using System.IO;
using System.Reflection;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 30465, "AspectFit Fails to Preserve Image Stretching on iOS and macOS After Downsizing",
        PlatformAffected.iOS | PlatformAffected.macOS)]
    public partial class Issue30465 : ContentPage
    {
        public Issue30465()
        {
            InitializeComponent();
            LoadImageFromFile();
        }

        void LoadImageFromFile()
        {
            try
            {
                // Simulate the scenario: load an image and save to cache, then display from file
                // We'll use the existing dotnet_bot.png from the test resources
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Maui.Controls.Sample.Resources.Images.dotnet_bot.png";
                
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        // Read the image data 
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            var imageData = memoryStream.ToArray();
                            
                            // Save to cache directory
                            var cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ImageCache");
                            Directory.CreateDirectory(cacheDir);
                            
                            var cachedImagePath = Path.Combine(cacheDir, "test_image.png");
                            File.WriteAllBytes(cachedImagePath, imageData);
                            
                            // Load the image from file using ImageSource.FromFile()
                            TestImage.Source = ImageSource.FromFile(cachedImagePath);
                            
                            StatusLabel.Text = $"Image loaded from file: {Path.GetFileName(cachedImagePath)}";
                        }
                    }
                    else
                    {
                        StatusLabel.Text = "Failed to load embedded resource";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusLabel.Text = $"Error: {ex.Message}";
            }
        }
    }
}