using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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
