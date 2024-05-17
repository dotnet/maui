using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class LabelScenario1 : PerformanceScenario
	{
		public LabelScenario1()
		: base("[Label] Empty")
		{
			View = new Label();
		}
	}
}
