using System;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Native.Gtk;

namespace Microsoft.Maui
{

	public static class ViewExtensions
	{

		public static void UpdateAutomationId(this Widget nativeView, IView view)
		{ }

		public static void UpdateBackground(this Widget nativeView, IView view)
		{
			var color = view.Background?.BackgroundColor;

			if (view.Background is { } paint)
			{
				color = paint.ToColor();
			}

			var css = view.Background.ToCss();

			var disposePixbuf = false;
			var pixbuf = css == null ? view.Background?.ToPixbuf(out disposePixbuf) : default;

			// create a temporary file 
			var tempFile = pixbuf?.TempFileFor();

			if (tempFile != null)
			{
				// use the tempfile as url in css
				css = $"url('{tempFile}')";
			}

			if (color == null && css == null)
				return;

			switch (nativeView)
			{
				case ProgressBar:
					nativeView.SetColor(color, "background-color", "trough > progress");

					break;
				case ComboBox box:
					// no effect: box.SetColor(bkColor, "border-color");
					box.GetCellRendererText().SetBackground(color);

					break;
				default:
					if (css != null)
					{
						nativeView.SetStyleValue(css, "background-image");
					}
					else
					{
						nativeView.SetBackgroundColor(color);
					}

					break;
			}

			// Gtk.CssProvider translates the file of url() into Base64, so the file can safely deleted:
			tempFile?.Dispose();

			if (disposePixbuf)
				pixbuf?.Dispose();
		}

		public static void UpdateForeground(this Widget nativeView, Paint? paint)
		{
			if (paint == null)
				return;

			var color = paint.ForegroundColor;

			if (paint.ToColor() is { } cp)
				color = cp;

			if (color == null)
				return;

			switch (nativeView)
			{
				case ProgressBar:
					nativeView.SetColor(color, "color", "trough > progress");

					break;
				case CheckButton:
					// no effect as check is an icon
					nativeView.SetColor(color, "color", "check");

					break;
				case ComboBox box:
					box.GetCellRendererText().SetForeground(color);

					break;
				default:
					nativeView.SetForegroundColor(color);

					break;
			}

		}

		public static void UpdateIsEnabled(this Widget nativeView, IView view) =>
			nativeView?.UpdateIsEnabled(view.IsEnabled);

		public static void UpdateVisibility(this Widget nativeView, IView view) =>
			nativeView?.UpdateVisibility(view.Visibility);

		public static void UpdateSemantics(this Widget nativeView, IView view)
		{
			if (view.Semantics is not { } semantics)
				return;

			nativeView.TooltipText = semantics.Hint;

			if (nativeView.Accessible is { } accessible and not Atk.NoOpObject && semantics.Description != null)
			{
				accessible.Description = semantics.Description;
			}

		}

		public static void UpdateOpacity(this Widget nativeView, IView view)
		{
			nativeView.Opacity = view.Opacity;
		}

		public static void UpdateClip(this WrapperView nativeView, IView view)
		{
			nativeView.Clip = view.Clip;
		}

		[MissingMapper]
		public static void UpdateClip(this Widget nativeView, IView view)
		{ }

		[MissingMapper]
		public static void UpdateFlowDirection(this Widget nativeView, IView view)
		{ }

	}

}