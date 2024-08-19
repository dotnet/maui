using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

internal class FrameCoreGalleryPage : CoreGalleryPage<Frame>
{
	protected override bool SupportsFocus => false;

	protected override void InitializeElement(Frame element)
	{
		element.HeightRequest = 50;
		element.WidthRequest = 100;
		element.BorderColor = Colors.Olive;
	}

	protected override void Build()
	{
		base.Build();
	}
}
