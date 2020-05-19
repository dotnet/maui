using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
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
