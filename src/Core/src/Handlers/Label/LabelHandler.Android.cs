using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class LabelHandler : ViewHandler<ILabel, AppCompatTextView>
	{
		static Color? DefaultTextColor { get; set; }
		static float? LineSpacingAddDefault { get; set; }
		static float? LineSpacingMultDefault { get; set; }

		protected override AppCompatTextView CreateNativeView() => new AppCompatTextView(Context);

		public override void NativeArrange(Rectangle frame)
		{
			var nativeView = this.ToPlatform();

			if (nativeView == null || Context == null)
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
				nativeView.Measure(MakeMeasureSpecExact(frame.Width), MakeMeasureSpecExact(frame.Height));
			}

			base.NativeArrange(frame);
		}

		protected override void ConnectHandler(AppCompatTextView nativeView)
		{
			base.ConnectHandler(nativeView);
			SetupDefaults(nativeView);
		}

		void SetupDefaults(AppCompatTextView nativeView)
		{
			if (nativeView.TextColors == null)
			{
				DefaultTextColor = null;
			}
			else
			{
				DefaultTextColor = Color.FromUint((uint)nativeView.TextColors.DefaultColor);
			}

			LineSpacingAddDefault = nativeView.LineSpacingExtra;
			LineSpacingMultDefault = nativeView.LineSpacingMultiplier;
		}

		public static void MapText(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextPlainText(label);
		}

		public static void MapTextColor(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextColor(label, DefaultTextColor);
		}

		public static void MapCharacterSpacing(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateCharacterSpacing(label);
		}

		public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateHorizontalTextAlignment(label);
		}

		public static void MapVerticalTextAlignment(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateVerticalTextAlignment(label);
		}

		public static void MapLineBreakMode(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateLineBreakMode(label);
		}

		public static void MapMaxLines(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateMaxLines(label);
		}

		public static void MapPadding(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdatePadding(label);
		}

		public static void MapTextDecorations(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateTextDecorations(label);
		}

		public static void MapFont(LabelHandler handler, ILabel label)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.NativeView?.UpdateFont(label, fontManager);
		}

		public static void MapLineHeight(LabelHandler handler, ILabel label)
		{
			handler.NativeView?.UpdateLineHeight(label);
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