using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void MapDetectReadingOrderFromContent(LabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateDetectReadingOrderFromContent(handler.PlatformView, label);

		public static void MapTextType(LabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.PlatformView, label);

		public static void MapText(LabelHandler handler, Label label) =>
			Platform.TextBlockExtensions.UpdateText(handler.PlatformView, label);

		public static void MapLineBreakMode(ILabelHandler handler, Label label) =>
			handler.PlatformView?.UpdateLineBreakMode(label.LineBreakMode);

		public static void MapMaxLines(ILabelHandler handler, Label label) =>
			handler.PlatformView?.UpdateMaxLines(label);

		private protected override void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			base.OnHandlerChangingCore(args);

			if (args.OldHandler?.PlatformView is TextBlock oldTextView)
			{
				oldTextView.LayoutUpdated -= OldTextView_LayoutUpdated;
				oldTextView.SizeChanged -= OldTextView_SizeChanged;
			}

			if (args.NewHandler?.PlatformView is TextBlock newTextView)
			{
				newTextView.LayoutUpdated += OldTextView_LayoutUpdated;
				newTextView.SizeChanged += OldTextView_SizeChanged;

			}
		}

		private void OldTextView_SizeChanged(object sender, UI.Xaml.SizeChangedEventArgs e)
		{
		}

		private void OldTextView_LayoutUpdated(object sender, object e)
		{
		}
	}
}
