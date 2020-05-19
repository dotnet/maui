using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class SliderScenario1 : PerformanceScenario
	{
		public SliderScenario1()
		: base("[Slider] Empty")
		{
			View = new Slider();
		}
	}
}
