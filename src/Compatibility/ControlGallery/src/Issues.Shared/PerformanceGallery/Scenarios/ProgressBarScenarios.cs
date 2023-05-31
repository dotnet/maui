using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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
