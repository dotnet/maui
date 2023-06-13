using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class SwitchScenario1 : PerformanceScenario
	{
		public SwitchScenario1()
		: base("[Switch] Empty")
		{
			View = new Switch();
		}
	}
}
