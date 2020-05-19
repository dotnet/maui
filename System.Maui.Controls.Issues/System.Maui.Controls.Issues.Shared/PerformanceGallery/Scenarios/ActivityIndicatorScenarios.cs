using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class ActivityIndicatorScenario1 : PerformanceScenario
	{
		public ActivityIndicatorScenario1()
		: base("[ActivityIndicator] Running")
		{
			View = new ActivityIndicator { IsRunning = true };
		}
	}
}
