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

		{
			var image = new Image
			{
				Source = new FontImageSource { FontFamily = "FA", Glyph = "\xf133", Size = 48, Color = Colors.Black },
				WidthRequest = 48,
				HeightRequest = 48,
			};
			var familyContainer = new StateViewContainer<Image>(Test.Image.Source_FontImageSource, image, o => ((FontImageSource)o).FontFamily);
			familyContainer.StateChangeButton.Clicked += (s, a) =>
			{
				if (image.Source is FontImageSource fis && fis.FontFamily == "FA")
					image.Source = new FontImageSource { FontFamily = "Ion", Glyph = "\xf30c", Size = 48, Color = Colors.Black };
				else
					image.Source = new FontImageSource { FontFamily = "FA", Glyph = "\xf133", Size = 48, Color = Colors.Black };
			};
			Add(familyContainer);
		}

		{
			var image = new Image
			{
				Source = "red_is_good.gif",
				WidthRequest = 100,
				HeightRequest = 100,
			};
			var container = new StateViewContainer<Image>(Test.Image.IsAnimationPlaying, image);
			container.StateChangeButton.Clicked += (s, a) =>
			{
				image.IsAnimationPlaying = !image.IsAnimationPlaying;
			};
			Add(container);
		}
	}
}
