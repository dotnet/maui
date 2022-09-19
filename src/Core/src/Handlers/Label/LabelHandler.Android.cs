using Android.Views;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, AppCompatTextView>
	{
		protected override AppCompatTextView CreatePlatformView()
			=> new MauiTextView(Context);

		public override void PlatformArrange(Rect frame)
		{
			var platformView = this.ToPlatform();

			if (platformView == null || Context == null)
			{
				return;
			}

			if (frame.Width < 0 || frame.Height < 0)
			{
				return;
			}

			// Depending on our layout situation, the TextView may need an additional measurement pass at the final size
			// in order to properly handle any TextAlignment properties.
			if (NeedsExactMeasure())
			{
				platformView.Measure(MakeMeasureSpecExact(frame.Width), MakeMeasureSpecExact(frame.Height));
			}

			base.PlatformArrange(frame);
		}

		internal static void MapBackground(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateBackground(label);
		}

		public static void MapText(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextPlainText(label);
		}

		public static void MapTextColor(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextColor(label);
		}

		public static void MapCharacterSpacing(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateCharacterSpacing(label);
		}

		public static void MapHorizontalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapVerticalTextAlignment(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateVerticalTextAlignment(label);
		}

		public static void MapPadding(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdatePadding(label);
		}

		public static void MapTextDecorations(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateTextDecorations(label);
		}

		public static void MapFont(ILabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(label, fontManager);
		}

		public static void MapLineHeight(ILabelHandler handler, ILabel label)
		{
			handler.PlatformView?.UpdateLineHeight(label);
		}

		bool NeedsExactMeasure()
		{
			if (VirtualView.VerticalLayoutAlignment != Primitives.LayoutAlignment.Fill
				&& VirtualView.HorizontalLayoutAlignment != Primitives.LayoutAlignment.Fill)
			{
				// Layout Alignments of Start, Center, and End will be laying out the TextView at its measured size,
				// so we won't need another pass with MeasureSpecMode.Exactly
				return false;
			}

			if (VirtualView.Width >= 0 && VirtualView.Height >= 0)
			{
				// If the Width and Height are both explicit, then we've already done MeasureSpecMode.Exactly in 
				// both dimensions; no need to do it again
				return false;
			}

			// We're going to need a second measurement pass so TextView can properly handle alignments
			return true;
		}

		int MakeMeasureSpecExact(double size)
		{
			// Convert to a native size to create the spec for measuring
			var deviceSize = (int)Context!.ToPixels(size);
			return MeasureSpecMode.Exactly.MakeMeasureSpec(deviceSize);
		}
	}
}