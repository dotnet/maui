using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
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
