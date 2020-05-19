using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class SearchBarScenario1 : PerformanceScenario
	{
		public SearchBarScenario1()
		: base("[SearchBar] Empty")
		{
			View = new SearchBar();
		}
	}
}
