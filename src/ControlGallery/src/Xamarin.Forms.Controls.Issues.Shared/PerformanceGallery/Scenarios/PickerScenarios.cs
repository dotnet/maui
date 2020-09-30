using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class PickerScenario1 : PerformanceScenario
	{
		public PickerScenario1()
		: base("[Picker] Empty")
		{
			View = new Picker();
		}
	}
}
