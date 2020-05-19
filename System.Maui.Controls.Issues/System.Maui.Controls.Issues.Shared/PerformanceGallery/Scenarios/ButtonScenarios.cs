using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class ButtonScenario1 : PerformanceScenario
	{
		public ButtonScenario1()
		: base("[Button] Text set in ctor")
		{
			View = new Button { Text = "I am a button" };
		}
	}
}
