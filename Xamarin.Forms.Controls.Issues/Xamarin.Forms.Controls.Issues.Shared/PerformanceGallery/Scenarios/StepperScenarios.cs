using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.PerformanceGallery.Scenarios
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
