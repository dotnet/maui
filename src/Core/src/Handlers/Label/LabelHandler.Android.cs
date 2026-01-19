using Android.Text;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.TextView;
using Java.Lang;
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
			if (VirtualView.HorizontalLayoutAlignment != Primitives.LayoutAlignment.Fill &&
				PlatformView?.Layout is Layout layout &&
				layout.LineCount > 1)
			{
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
						return new Size(actualWidth, size.Height);
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

	// TODO: Material3 - make it public in .net 11
	internal class MauiMaterialTextView : MaterialTextView
	{
		// Track text type to optimize layout events - FormattedText needs span recalculation, plain text doesn't
		bool _isFormatted;

		public MauiMaterialTextView(Context context) : base(MauiMaterialContextThemeWrapper.Create(context))
		{
		}

		internal event EventHandler<LayoutChangedEventArgs>? LayoutChanged;

		public override void SetText(ICharSequence? text, BufferType? type)
		{
			// Detect FormattedText (SpannableString) vs plain text to avoid unnecessary layout events
			_isFormatted = text is not Java.Lang.String;
			base.SetText(text, type);
		}

		protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
		{
			base.OnLayout(changed, left, top, right, bottom);

			// Only notify for FormattedText - spans need position updates when layout changes
			if (_isFormatted)
			{
				LayoutChanged?.Invoke(this, new LayoutChangedEventArgs(left, top, right, bottom));
			}
		}
	}
}