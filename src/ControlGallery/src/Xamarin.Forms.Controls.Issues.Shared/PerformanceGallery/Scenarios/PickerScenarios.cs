using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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
