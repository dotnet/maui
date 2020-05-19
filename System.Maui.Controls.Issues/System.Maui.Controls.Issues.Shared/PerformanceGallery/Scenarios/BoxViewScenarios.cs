using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
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
