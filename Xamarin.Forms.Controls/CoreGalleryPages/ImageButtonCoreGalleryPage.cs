using System;

using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class ImageButtonCoreGalleryPage : CoreGalleryPage<ImageButton>
	{
		protected override bool SupportsFocus => false;

		protected override bool SupportsTapGestureRecognizer => false;

		protected override void InitializeElement(ImageButton element)
		{
			element.Source = "oasissmall.jpg";
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);


			IsEnabledStateViewContainer.View.Clicked += (sender, args) =>
			{
				IsEnabledStateViewContainer.TitleLabel.Text += " (Tapped)";
			};

			var aspectFillContainer = new ViewContainer<ImageButton>(Test.ImageButton.AspectFill, new ImageButton { Aspect = Aspect.AspectFill });
			var aspectFitContainer = new ViewContainer<ImageButton>(Test.ImageButton.AspectFit, new ImageButton { Aspect = Aspect.AspectFit });
			var fillContainer = new ViewContainer<ImageButton>(Test.ImageButton.Fill, new ImageButton { Aspect = Aspect.Fill });
			var isLoadingContainer = new StateViewContainer<ImageButton>(Test.ImageButton.IsLoading, new ImageButton());
			var isOpaqueContainer = new StateViewContainer<ImageButton>(Test.ImageButton.IsOpaque, new ImageButton());


			var borderButtonContainer = new ViewContainer<ImageButton>(Test.ImageButton.BorderColor,
				new ImageButton
				{
					BackgroundColor = Color.Transparent,
					BorderColor = Color.Red,
					BorderWidth = 1,
					Source = "oasissmall.jpg"
				}
			);

			var corderRadiusContainer = new ViewContainer<ImageButton>(Test.ImageButton.CornerRadius,
				new ImageButton
				{
					Source = "oasissmall.jpg",
					BackgroundColor = Color.Transparent,
					BorderColor = Color.Red,
					CornerRadius = 20,
					BorderWidth = 1,
				}
			);

			var borderWidthContainer = new ViewContainer<ImageButton>(Test.ImageButton.BorderWidth,
				new ImageButton
				{
					Source = "oasissmall.jpg",
					BackgroundColor = Color.Transparent,
					BorderColor = Color.Red,
					BorderWidth = 15,
				}
			);

			var clickedContainer = new EventViewContainer<ImageButton>(Test.ImageButton.Clicked,
				new ImageButton
				{
					Source = "oasissmall.jpg"
				}
			);
			clickedContainer.View.Clicked += (sender, args) => clickedContainer.EventFired();

			var pressedContainer = new EventViewContainer<ImageButton>(Test.ImageButton.Pressed,
				new ImageButton
				{
					Source = "oasissmall.jpg"
				}
			);
			pressedContainer.View.Pressed += (sender, args) => pressedContainer.EventFired();

			var commandContainer = new ViewContainer<ImageButton>(Test.ImageButton.Command,
				new ImageButton
				{
					Command = new Command(() => DisplayActionSheet("Hello Command", "Cancel", "Destroy")),
					Source = "oasissmall.jpg"
				}
			);

			var imageContainer = new ViewContainer<ImageButton>(Test.ImageButton.Image,
				new ImageButton
				{
					Source = new FileImageSource { File = "bank.png" }
				}
			);


			InitializeElement(aspectFillContainer.View);
			InitializeElement(aspectFitContainer.View);
			InitializeElement(fillContainer.View);
			InitializeElement(isLoadingContainer.View);
			InitializeElement(isOpaqueContainer.View);

			var sourceContainer = new ViewContainer<ImageButton>(Test.ImageButton.Source, new ImageButton { Source = "https://raw.githubusercontent.com/xamarin/Xamarin.Forms/master/Xamarin.Forms.Controls/coffee.png" });

			Add(aspectFillContainer);
			Add(aspectFitContainer);
			Add(fillContainer);
			Add(isLoadingContainer);
			Add(isOpaqueContainer);
			Add(sourceContainer);

			Add(borderButtonContainer);
			Add(borderWidthContainer);
			Add(clickedContainer);
			Add(commandContainer);
			Add(corderRadiusContainer);
			Add(imageContainer);
			Add(pressedContainer);
		}
	}
}