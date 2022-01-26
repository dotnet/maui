using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics.Platform.Gtk {

	public partial class PlatformCanvas : AbstractCanvas<PlatformCanvasState> {

		internal TextLayout CreateTextLayout() {
			var layout = new TextLayout(Context)
			   .WithCanvasState(CurrentState);

			layout.BeforeDrawn = LayoutBeforeDrawn;
			layout.AfterDrawn = LayoutAfterDrawn;

			return layout;
		}

		private void LayoutBeforeDrawn(TextLayout layout) {
			DrawFillPaint(Context, CurrentState.FillPaint.paint, CurrentState.FillPaint.rectangle);
		}

		private void LayoutAfterDrawn(TextLayout layout) { }

		public override void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment) {

			using var layout = CreateTextLayout();
			layout.HorizontalAlignment = horizontalAlignment;

			layout.DrawString(value, x, y);

		}

		public override void DrawString(string value, float x, float y, float width, float height, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, TextFlow textFlow = TextFlow.ClipBounds, float lineSpacingAdjustment = 0) {

			using var layout = CreateTextLayout();
			layout.HorizontalAlignment = horizontalAlignment;
			layout.VerticalAlignment = verticalAlignment;
			layout.TextFlow = textFlow;
			layout.LineSpacingAdjustment = lineSpacingAdjustment;

			layout.DrawString(value, x, y, width, height);

		}

		[GtkMissingImplementation]
		public override void DrawText(IAttributedText value, float x, float y, float width, float height) {
			using var layout = CreateTextLayout();

			layout.DrawAttributedText(value, x, y, width, height);

		}

		private static TextLayout? _textLayout;

		private TextLayout SharedTextLayout => _textLayout ??= new TextLayout(SharedContext)
		{
			HeightForWidth = true
		};

		public override SizeF GetStringSize(string value, IFont font, float textWidth)
		{
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			lock (SharedTextLayout)
			{
				SharedTextLayout.FontFamily = font?.Name ?? FontExtensions.Default.Family;

				return SharedTextLayout.GetSize(value, textWidth);
			}

		}

		public override SizeF GetStringSize(string value, IFont font, float textWidth, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
		{
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			lock (SharedTextLayout)
			{
				SharedTextLayout.FontFamily = font.Name;
				SharedTextLayout.HorizontalAlignment = horizontalAlignment;
				SharedTextLayout.VerticalAlignment = verticalAlignment;

				return SharedTextLayout.GetSize(value, textWidth);
			}
		}
	}

}
