using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
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
