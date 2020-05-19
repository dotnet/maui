using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
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
