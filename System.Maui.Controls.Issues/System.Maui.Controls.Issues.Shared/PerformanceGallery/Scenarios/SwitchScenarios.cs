using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
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
