using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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
