using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	public partial class NativeCanvas {

		public TextLayout CreateTextLayout() {
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

	}

}
