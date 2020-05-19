using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class DatePickerScenario1 : PerformanceScenario
	{
		public DatePickerScenario1()
		: base("[DatePicker] Empty")
		{
			View = new DatePicker();
		}
	}
}
