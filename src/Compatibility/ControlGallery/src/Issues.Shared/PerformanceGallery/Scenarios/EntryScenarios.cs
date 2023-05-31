using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class EntryScenario1 : PerformanceScenario
	{
		public EntryScenario1()
		: base("[Entry] Empty")
		{
			View = new Entry();
		}
	}
}
