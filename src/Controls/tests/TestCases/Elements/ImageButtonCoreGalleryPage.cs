using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class ImageButtonCoreGalleryPage : CoreGalleryPage<ImageButton>
{
	protected override bool SupportsTapGestureRecognizer => false;

	protected override void InitializeElement(ImageButton element)
	{
		element.Source = "small_dotnet_bot.png";
	}

	protected override void Build()
	{
		base.Build();
	}
}
