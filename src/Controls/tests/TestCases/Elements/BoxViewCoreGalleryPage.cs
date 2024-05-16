using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

internal class BoxViewCoreGalleryPage : CoreGalleryPage<BoxView>
{
	protected override bool SupportsFocus => false;

	protected override void InitializeElement(BoxView element)
	{
		element.HeightRequest = 200;

		element.Color = Colors.Purple;
	}

	protected override void Build()
	{
		base.Build();
	}
}
