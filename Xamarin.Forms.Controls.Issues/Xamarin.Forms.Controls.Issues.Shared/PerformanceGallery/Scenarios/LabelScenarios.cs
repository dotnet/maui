using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class LabelScenario1 : PerformanceScenario
	{
		public LabelScenario1()
		: base("[Label] Empty")
		{
			View = new Label();
		}
	}
}
