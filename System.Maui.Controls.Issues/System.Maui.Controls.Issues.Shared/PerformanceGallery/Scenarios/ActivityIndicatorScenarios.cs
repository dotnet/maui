using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
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
