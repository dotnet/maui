using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class PickerCoreGalleryPage : CoreGalleryPage<Picker>
{
	protected override bool SupportsTapGestureRecognizer => false;

	protected override void InitializeElement(Picker element)
	{
	}

	protected override void Build()
	{
		base.Build();
	}
}
