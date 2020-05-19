using System;
using System.Linq;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.PerformanceGallery.Scenarios
{
	[Preserve(AllMembers = true)]
	internal class StepperScenario1 : PerformanceScenario
	{
		public StepperScenario1()
		: base("[Stepper] Empty")
		{
			View = new Stepper();
		}
	}
}
