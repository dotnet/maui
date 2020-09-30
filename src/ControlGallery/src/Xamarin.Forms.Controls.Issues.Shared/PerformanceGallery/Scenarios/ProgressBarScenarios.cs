using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class ProgressBarScenario1 : PerformanceScenario
	{
		public ProgressBarScenario1()
		: base("[ProgressBar] Empty")
		{
			View = new ProgressBar();
		}
	}
}
