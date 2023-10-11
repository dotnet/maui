using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class StepperCoreGalleryPage : CoreGalleryPage<Stepper>
{
	protected override bool SupportsFocus => false;
	protected override bool SupportsTapGestureRecognizer => false;

	protected override void InitializeElement(Stepper element)
	{
	}

	protected override void Build()
	{
		base.Build();
	}
}
