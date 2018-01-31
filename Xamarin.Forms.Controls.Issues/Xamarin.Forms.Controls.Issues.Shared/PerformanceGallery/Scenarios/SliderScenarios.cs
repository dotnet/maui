using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
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
