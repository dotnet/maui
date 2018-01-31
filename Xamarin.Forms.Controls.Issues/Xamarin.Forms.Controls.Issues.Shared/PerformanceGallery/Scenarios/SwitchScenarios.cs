using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class SwitchScenario1 : PerformanceScenario
	{
		public SwitchScenario1()
		: base("[Switch] Empty")
		{
			View = new Switch();
		}
	}
}
