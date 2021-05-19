namespace Microsoft.Maui.Graphics.Native.Gtk {

	public partial class NativeCanvas {

		public void DrawShadow(bool fill) {

			if (CurrentState.Shadow != default) {

				using var path = Context.CopyPath();

				Context.Save();

				var shadowSurface = CreateSurface(Context);

				var shadowCtx = new Cairo.Context(shadowSurface);

				var shadow = CurrentState.Shadow;

				shadowCtx.AppendPath(path);

				if (fill)
					shadowCtx.ClosePath();

				var color = shadow.color.ToCairoColor();
				shadowCtx.SetSourceRGBA(color.R, color.G, color.B, color.A);
				shadowCtx.Clip();

				if (true)
					shadowCtx.PaintWithAlpha(0.3);
				else {
					shadowCtx.LineWidth = 10;
					shadowCtx.Stroke();
				}

				// shadowCtx.PopGroupToSource();
				Context.SetSource(shadowSurface, shadow.offset.Width, shadow.offset.Height);
				Context.Paint();

				shadowCtx.Dispose();

				shadowSurface.Dispose();

				Context.Restore();
			}
		}

	}

}
