using System;
using System.Numerics;
using System.Threading.Tasks;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Action = System.Action;

namespace Microsoft.Maui.Platform
{

	public static partial class ViewExtensions
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

		[MissingMapper]
		public static Task UpdateBackgroundImageSourceAsync(this Widget platformView, IImageSource? imageSource, IImageSourceServiceProvider? provider)
			=> Task.CompletedTask;

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

		internal static Rect GetBoundingBox(this IView view)
			=> view.ToPlatform().GetBoundingBox();

		internal static Graphics.Rect GetPlatformViewBounds(this IView view)
		{
			var w = view.ToPlatform();

			return w.GetPlatformViewBounds();

		}

		internal static Rect GetPlatformViewBounds(this Gtk.Widget? platformView)
		{
			if (platformView?.Toplevel is not { } tl)
				return platformView.GetBoundingBox();

			platformView.TranslateCoordinates(tl, 0, 0, out var x, out var y);

			return new Rect(x, y, platformView.AllocatedWidth, platformView.AllocatedHeight);
		}

		internal static Rect GetBoundingBox(this Gtk.Widget? platformView)
			=> platformView is not { } ? new Rect() : platformView.Allocation.ToRect();

		internal static Matrix4x4 GetViewTransform(this IView view)
		{
			var platformView = view?.ToPlatform();

			if (platformView == null)
				return new Matrix4x4();

			return platformView.GetViewTransform();
		}

		internal static Matrix4x4 GetViewTransform(this Widget view)
		{
			if (view == null)
				return new Matrix4x4();

			var bounds = view.GetPlatformViewBounds();
			var scale = view.ScaleFactor;

			return new Matrix4x4()
			{
				// TODO: add scale
				Translation = new Vector3((float)bounds.X, (float)bounds.X, 0)
			};
		}

		internal static Widget? GetParent(this Widget? view)
		{
			return view?.Parent;
		}

		public static void UpdateShadow(this Widget? platformView, IView view) { }

		public static void UpdateBorder(this Widget? platformView, IView view) { }

		public static void Focus(this Widget? platformView, FocusRequest request) { }

		public static void Unfocus(this Widget? platformView, IView view) { }

		public static void UpdateInputTransparent(this Widget? platformView, IViewHandler handler, IView view) { }

		public static void UpdateToolTip(this Widget? platformView, ToolTip? tooltip)
		{ }

		[MissingMapper]
		internal static IDisposable OnLoaded(this Widget? platformView, Action action)
		{
			throw new NotImplementedException();
		}

		[MissingMapper]
		internal static IDisposable OnUnloaded(this Widget? platformView, Action action)
		{
			throw new NotImplementedException();
		}

		public static bool IsLoaded(this Widget? platformView)
		{
			if (platformView is not { })
				return false;

			return platformView.IsRealized;
		}

	}

}