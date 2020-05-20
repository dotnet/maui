using UIKit;

namespace System.Maui.Platform {
	public partial class ButtonRenderer {
		protected override UIButton CreateView()
		{
			var button = UIButton.FromType(UIButtonType.RoundedRect);
			button.TouchUpInside += Button_TouchUpInside;
			button.BackgroundColor = UIColor.Green;
			return button;
		}

		protected override void SetupDefaults()
		{
			DefaultTextColor = TypedNativeView.CurrentTitleColor.ToColor();
		}

		private void Button_TouchUpInside (object sender, EventArgs e)
		{
			this.VirtualView.Clicked ();
		}

		public override SizeRequest GetDesiredSize (double widthConstraint, double heightConstraint)
		{
			var size = TypedNativeView.SizeThatFits (new CoreGraphics.CGSize (widthConstraint, heightConstraint));
			return new SizeRequest( new Size(size.Width, size.Height));
		}

		protected override void DisposeView (UIButton nativeView)
		{
			nativeView.TouchUpInside -= Button_TouchUpInside;
			base.DisposeView (nativeView);
		}


		//public static void MapPropertyButtonFont(IViewRenderer renderer, IButton view)
		//{

		//}

		//public static void MapPropertyButtonInputTransparent(IViewRenderer renderer, IButton view)
		//{

		//}

		//public static void MapPropertyButtonCharacterSpacing(IViewRenderer renderer, IButton view)
		//{

		//}

	}
}
