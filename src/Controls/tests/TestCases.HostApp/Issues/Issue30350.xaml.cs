using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 30350, "IImage downsize broken starting from 9.0.80 and not fixed in 9.0.81", PlatformAffected.iOS)]
	public partial class Issue30350 : TestContentPage
	{
		private ImageSource _originalSource;
		private string _originalSize;
		private ImageSource _downsizedSource;
		private string _downsizedImageSize;

		public Issue30350()
		{
			InitializeComponent();
			BindingContext = this;
		}

		protected override void Init()
		{
			// Delay loading to ensure all controls are initialized
			Dispatcher.Dispatch(async () => await LoadImagesAsync());
		}

		public ImageSource OriginalSource
		{
			get => _originalSource;
			set
			{
				if (Equals(value, _originalSource))
					return;
				_originalSource = value;
				OnPropertyChanged();
			}
		}

		public ImageSource DownsizedSource
		{
			get => _downsizedSource;
			set
			{
				if (Equals(value, _downsizedSource))
					return;
				_downsizedSource = value;
				OnPropertyChanged();
			}
		}

		public string OriginalSize
		{
			get => _originalSize;
			set
			{
				if (value == _originalSize)
					return;
				_originalSize = value;
				OnPropertyChanged();
			}
		}

		public string DownsizedImageSize
		{
			get => _downsizedImageSize;
			set
			{
				if (value == _downsizedImageSize)
					return;
				_downsizedImageSize = value;
				OnPropertyChanged();
			}
		}

		private async Task LoadImagesAsync()
		{
			try
			{
				// Check if StatusLabel is available
				StatusLabel?.Text = "Loading images...";

				// Load original image from embedded resource  
				OriginalSource = ImageSource.FromResource("Controls.TestCases.HostApp.Resources.Images.royals.png");

				// Load IImage for processing
				var assembly = GetType().GetTypeInfo().Assembly;
				using var stream = assembly.GetManifestResourceStream("Controls.TestCases.HostApp.Resources.Images.royals.png");

				if (stream == null)
				{
					StatusLabel?.Text = "Error: Could not load royals.png resource";
					return;
				}

				var originalImage = PlatformImage.FromStream(stream);
				if (originalImage == null)
				{
					StatusLabel?.Text = "Error: Could not create IImage from stream";
					return;
				}

				OriginalSize = $"{originalImage.Width}x{originalImage.Height}";

				// Perform downsize operation - this is where the issue occurs in MAUI 9.0.80+
				// The downsized image should be flipped/rotated incorrectly on iOS 9.0.80+
				var downsizedImage = originalImage.Downsize(100);
				if (downsizedImage == null)
				{
					StatusLabel?.Text = "Error: Downsize operation failed";
					return;
				}

				DownsizedImageSize = $"{downsizedImage.Width}x{downsizedImage.Height}";

				// Convert downsized IImage to ImageSource
				using var downsizedStream = new MemoryStream();
				await downsizedImage.SaveAsync(downsizedStream, ImageFormat.Png);
				downsizedStream.Position = 0;

				DownsizedSource = ImageSource.FromStream(() => new MemoryStream(downsizedStream.ToArray()));

				StatusLabel?.Text = "Images loaded. Compare for flipping/rotation issues in MAUI 9.0.80+";
			}
			catch (Exception ex)
			{
				StatusLabel?.Text = $"Error loading images: {ex.Message}";
			}
		}
	}
}