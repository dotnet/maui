using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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
