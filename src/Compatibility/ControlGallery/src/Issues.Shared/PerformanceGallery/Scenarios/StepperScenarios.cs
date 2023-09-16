using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.PerformanceGallery.Scenarios
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
