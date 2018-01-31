using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class BoxViewScenario1 : PerformanceScenario
	{
		public BoxViewScenario1()
		: base("[BoxView] Color set in ctor")
		{
			View = new BoxView { Color = Color.Red };
		}
	}
}
