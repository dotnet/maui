using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	internal class ImageLoadingGalleryPage : CoreGalleryBasePage
	{
		protected override void Build()
		{
			Add(Test.ImageLoading.FromBundleSvg, ImageSource.FromFile("dotnet_bot.png"));

			Add(Test.ImageLoading.FromBundlePng, ImageSource.FromFile("groceries.png"));

			Add(Test.ImageLoading.FromBundleJpg, ImageSource.FromFile("oasis.jpg"));

			Add(Test.ImageLoading.FromBundleGif, ImageSource.FromFile("animated_heart.gif"));
		}

		ViewContainer<Image> Add(Test.ImageLoading test, ImageSource image) =>
			Add(test, new Image
			{
				Source = image,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Start
			});

		ViewContainer<Image> Add(Test.ImageLoading test, Image image) =>
			Add(new ViewContainer<Image>(test, image));
	}
}
