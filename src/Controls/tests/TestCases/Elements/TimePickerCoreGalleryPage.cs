using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class TimePickerCoreGalleryPage : CoreGalleryPage<TimePicker>
{
	protected override bool SupportsTapGestureRecognizer => false;

	protected override void InitializeElement(TimePicker element)
	{
	}

	protected override void Build()
	{
		base.Build();
	}
}
