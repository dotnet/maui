using System;

using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class ImageCoreGalleryPage : CoreGalleryPage<Image>
	{
		protected override bool SupportsFocus => false;

		protected override void InitializeElement(Image element)
		{
			element.Source = "oasissmall.jpg";
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var aspectFillContainer = new ViewContainer<Image>(Test.Image.AspectFill, new Image { Aspect = Aspect.AspectFill });
			var aspectFitContainer = new ViewContainer<Image>(Test.Image.AspectFit, new Image { Aspect = Aspect.AspectFit });
			var fillContainer = new ViewContainer<Image>(Test.Image.Fill, new Image { Aspect = Aspect.Fill });
			var isLoadingContainer = new StateViewContainer<Image>(Test.Image.IsLoading, new Image());
			var isOpaqueContainer = new StateViewContainer<Image>(Test.Image.IsOpaque, new Image());

			InitializeElement(aspectFillContainer.View);
			InitializeElement(aspectFitContainer.View);
			InitializeElement(fillContainer.View);
			InitializeElement(isLoadingContainer.View);
			InitializeElement(isOpaqueContainer.View);

			var sourceContainer = new ViewContainer<Image>(Test.Image.Source, new Image { Source = "https://raw.githubusercontent.com/xamarin/Xamarin.Forms/main/Xamarin.Forms.Controls/coffee.png" });

			Add(aspectFillContainer);
			Add(aspectFitContainer);
			Add(fillContainer);
			Add(isLoadingContainer);
			Add(isOpaqueContainer);
			Add(sourceContainer);
		}
	}
}