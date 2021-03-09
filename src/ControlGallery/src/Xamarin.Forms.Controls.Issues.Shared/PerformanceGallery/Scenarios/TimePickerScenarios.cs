using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class TimePickerScenario1 : PerformanceScenario
	{
		public TimePickerScenario1()
		: base("[TimePicker] Empty")
		{
			View = new TimePicker();
		}
	}
}
