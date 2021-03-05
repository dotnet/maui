using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : AbstractViewHandler<IEntry, UITextField>
	{
		static readonly int BaseHeight = 30;

		static UIColor? DefaultTextColor;

		protected override UITextField CreateNativeView()
		{
			return new UITextField(CGRect.Empty)
			{
				BorderStyle = UITextBorderStyle.RoundedRect,
				ClipsToBounds = true
			};
		}

		protected override void SetupDefaults(UITextField nativeView)
		{
			DefaultTextColor = nativeView.TextColor;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			new SizeRequest(new Size(widthConstraint, BaseHeight));

		public static void MapText(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateText(entry);
		}

		public static void MapTextColor(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateTextColor(entry, DefaultTextColor);
		}

		public static void MapIsPassword(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsPassword(entry);
		}

		public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdateIsTextPredictionEnabled(entry);
		}

		public static void MapPlaceholder(EntryHandler handler, IEntry entry)
		{
			handler.TypedNativeView?.UpdatePlaceholder(entry);
    }
	}
}