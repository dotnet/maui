using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
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
