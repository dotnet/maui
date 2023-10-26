using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class SliderCoreGalleryPage : CoreGalleryPage<Slider>
{
	protected override bool SupportsFocus => false;

	protected override bool SupportsTapGestureRecognizer => false;

	protected override void InitializeElement(Slider element)
	{
	}

	protected override void Build()
	{
		base.Build();
	}
}
