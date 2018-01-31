using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
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
