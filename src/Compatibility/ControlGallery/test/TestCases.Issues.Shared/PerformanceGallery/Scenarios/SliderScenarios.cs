using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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
