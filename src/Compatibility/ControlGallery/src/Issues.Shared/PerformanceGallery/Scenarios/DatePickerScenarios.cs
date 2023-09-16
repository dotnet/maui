using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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
