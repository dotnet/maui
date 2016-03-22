using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class FrameCoreGalleryPage : CoreGalleryPage<Frame>
	{
		// TODO
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement (Frame element)
		{
			element.HeightRequest = 50;
			element.WidthRequest = 100;
			element.OutlineColor = Color.Olive;
		}

		protected override void Build (StackLayout stackLayout)
		{
			base.Build (stackLayout);

			var hasShadowContainer = new StateViewContainer<Frame> (Test.Frame.HasShadow, new Frame { HasShadow = true });
			var outlineColorContainer = new StateViewContainer<Frame> (Test.Frame.OutlineColor, new Frame { OutlineColor = Color.Teal, });
			var viewContainer = new StateViewContainer<Frame> (Test.Frame.OutlineColor, new Frame {
				OutlineColor = Color.Teal,
				Content = new Label { Text = "I am a frame" }
			});

			Add (hasShadowContainer);
			Add (outlineColorContainer);
			Add (viewContainer);
		}
	}
}