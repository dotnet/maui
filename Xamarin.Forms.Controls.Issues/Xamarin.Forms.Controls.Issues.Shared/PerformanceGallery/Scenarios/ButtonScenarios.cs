using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
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
