using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
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
