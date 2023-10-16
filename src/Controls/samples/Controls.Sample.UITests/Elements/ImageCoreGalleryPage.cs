using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

internal class ImageCoreGalleryPage : CoreGalleryPage<Image>
{
	protected override bool SupportsFocus => false;

	protected override void InitializeElement(Image element)
	{
		element.Source = "small_dotnet_bot.png";
	}

	protected override void Build()
	{
		base.Build();
	}
}
