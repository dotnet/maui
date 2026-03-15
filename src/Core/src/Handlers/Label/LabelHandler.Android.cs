using Android.Text;
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
			this.PrepareForTextViewArrange(frame);
			base.PlatformArrange(frame);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var size = base.GetDesiredSize(widthConstraint, heightConstraint);

			// Android TextView reports full available width instead of actual text width when
			// text wraps to multiple lines, causing incorrect positioning for non-Fill alignments.
			// We narrow the desired width to the widest rendered line, but only when that narrowing
			// won't cause re-wrapping that exceeds MaxLines and truncates visible text.
			if (VirtualView.HorizontalLayoutAlignment != Primitives.LayoutAlignment.Fill &&
				PlatformView?.Layout is Layout layout &&
				layout.LineCount > 1 &&
				PlatformView.Ellipsize == null)
			{
				// Capture original line count before narrowing to detect re-wrapping.
				var originalLineCount = layout.LineCount;

				float maxLineWidth = 0;
				for (int i = 0; i < layout.LineCount; i++)
				{
					float lineWidth = layout.GetLineWidth(i);
					if (lineWidth > maxLineWidth)
						maxLineWidth = lineWidth;
				}

				if (maxLineWidth > 0)
				{
					var actualWidth = Context.FromPixels(maxLineWidth + PlatformView.PaddingLeft + PlatformView.PaddingRight);
					if (actualWidth < size.Width)
					{
						// Always verify that narrowing doesn't cause the text to re-wrap into more
						// lines than the original measurement, which would truncate visible text.
						// This covers both explicit MaxLines constraints and cases where layout
						// under-reports line widths (e.g. RTL/bidi text in an RTL container).
						// Re-measure at exactly the pixel width the view will be arranged at.
						var narrowedPx = (int)Context.ToPixels(actualWidth);

						// AtMost mirrors how the layout pass constrains width, ensuring the
						// re-measurement reflects the same wrapping behaviour the view will
						// experience when arranged at actualWidth.
						PlatformView.Measure(
							MeasureSpecMode.AtMost.MakeMeasureSpec(narrowedPx),
							MeasureSpecMode.Unspecified.MakeMeasureSpec(0));

						// Fail-safe: if Layout is null after re-measurement we cannot verify
						// that truncation won't occur, so return the original size.
						var measuredLayout = PlatformView.Layout;

						// Use MaxLines as the limit when explicitly set; otherwise use the
						// original line count so that any re-wrapping caused by the narrowed
						// width is detected and the original size is returned instead.
						var lineLimit = PlatformView.MaxLines != int.MaxValue
							? PlatformView.MaxLines
							: originalLineCount;

						if (measuredLayout is null || measuredLayout.LineCount > lineLimit)
						{
							return size; // Narrowing causes truncation (or unverifiable); return original size
						}

						return new Size(actualWidth, size.Height);
					}
				}
			}

			return size;
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
	}

	// TODO: Material3 - make it public in .net 11
	internal class LabelHandler2 : LabelHandler
	{
		protected override MauiMaterialTextView CreatePlatformView()
		{
			return new MauiMaterialTextView(Context);
		}
	}
}
