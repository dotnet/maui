using System;
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
}
