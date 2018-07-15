using System;
using System.IO;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class ImageScenario1 : PerformanceScenario
	{
		public ImageScenario1()
		: base("[Image] Empty")
		{
			View = new Image();
		}
	}

	[Preserve(AllMembers = true)]
	internal class ImageScenario2 : PerformanceScenario
	{
		public ImageScenario2()
		: base("[Image] Embedded source")
		{
			View = new Image { Source = "coffee.png" };
		}
	}

	[Preserve(AllMembers = true)]
	internal class ImageScenario3 : PerformanceScenario
	{
		const int count = 5;

		public ImageScenario3()
		: base($"[Image] {count}x AndroidResource")
		{
			var source = ImageSource.FromFile("bank.png");
			var layout = new StackLayout();
			for (int i = 0; i < count; i++)
			{
				layout.Children.Add(new Image { Source = source, HeightRequest = 20 });
			}
			View = layout;
		}
	}

	[Preserve(AllMembers = true)]
	internal class ImageScenario4 : PerformanceScenario
	{
		const int count = 5;
		static readonly string tempFile;

		static ImageScenario4()
		{
			//NOTE: copy image to disk in static ctor, so not to interfere with timing
			tempFile = Path.Combine(Path.GetTempPath(), $"{nameof(ImageScenario4)}.png");
			using (var embeddedStream = typeof(ImageScenario4).Assembly.GetManifestResourceStream("Xamarin.Forms.Controls.GalleryPages.crimson.jpg"))
			using (var fileStream = File.Create(tempFile))
				embeddedStream.CopyTo(fileStream);
		}

		public ImageScenario4()
		: base($"[Image] {count}x from disk")
		{
			var source = ImageSource.FromFile(tempFile);
			var layout = new StackLayout();
			for (int i = 0; i < count; i++)
			{
				layout.Children.Add(new Image { Source = source, HeightRequest = 20 });
			}
			View = layout;
		}
	}
}
