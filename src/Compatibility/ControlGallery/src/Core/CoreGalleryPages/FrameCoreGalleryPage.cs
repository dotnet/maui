using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery
{
	internal class FrameCoreGalleryPage : CoreGalleryPage<Frame>
	{
		// TODO
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement(Frame element)
		{
			element.HeightRequest = 50;
			element.WidthRequest = 100;
			element.BorderColor = Colors.Olive;
		}

		readonly Random _randomValue = new Random();
		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

			var hasShadowContainer = new StateViewContainer<Frame>(Test.Frame.HasShadow, new Frame { HasShadow = true });
			hasShadowContainer.StateChangeButton.Command = new Command(() => hasShadowContainer.View.HasShadow = !hasShadowContainer.View.HasShadow);

			var viewContainer = new StateViewContainer<Frame>(Test.Frame.Content, new Frame
			{
				BorderColor = Colors.Teal,
				Content = new Label { Text = "I am a frame" }
			});
			viewContainer.StateChangeButton.Command = new Command(() => viewContainer.View.Content = new Label { Text = "Different content" });

			var cornerRadiusContainer = new StateViewContainer<Frame>(Test.Frame.CornerRadius, new Frame
			{
				BorderColor = Colors.Teal,
				CornerRadius = 25
			});
			cornerRadiusContainer.StateChangeButton.Command = new Command(() => cornerRadiusContainer.View.CornerRadius = _randomValue.Next(0, 25));

			Add(hasShadowContainer);
			Add(viewContainer);
			Add(cornerRadiusContainer);
		}
	}
}