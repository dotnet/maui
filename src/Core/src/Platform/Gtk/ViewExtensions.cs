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
		public static void UpdateAutomationId(this Widget platformView, IView view) { }

		public static void UpdateBackground(this Widget platformView, IView view)
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

			switch (platformView)
			{
				case ProgressBar:
					platformView.SetColor(color, "background-color", "trough > progress");

					break;
				case ComboBox box:
					// no effect: box.SetColor(bkColor, "border-color");
					box.GetCellRendererText().SetBackground(color);

					break;
				default:
					if (css != null)
					{
						platformView.SetStyleValue(css, "background-image");
					}
					else
					{
						platformView.SetBackgroundColor(color);
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

		public static void UpdateForeground(this Widget platformView, Paint? paint)
		{
			if (paint == null)
				return;

			var color = paint.ForegroundColor;

			if (paint.ToColor() is { } cp)
				color = cp;

			if (color == null)
				return;

			switch (platformView)
			{
				case ProgressBar:
					platformView.SetColor(color, "color", "trough > progress");

					break;
				case CheckButton:
					// no effect as check is an icon
					platformView.SetColor(color, "color", "check");

					break;
				case ComboBox box:
					box.GetCellRendererText().SetForeground(color);

					break;
				default:
					platformView.SetForegroundColor(color);

					break;
			}
		}

		public static void UpdateIsEnabled(this Widget platformView, IView view) =>
			platformView?.UpdateIsEnabled(view.IsEnabled);

		public static void UpdateVisibility(this Widget platformView, IView view) =>
			platformView?.UpdateVisibility(view.Visibility);

		public static void UpdateSemantics(this Widget platformView, IView view)
		{
			if (view.Semantics is not { } semantics)
				return;

			platformView.TooltipText = semantics.Hint;

			if (platformView.Accessible is { } accessible and not Atk.NoOpObject && semantics.Description != null)
			{
				accessible.Description = semantics.Description;
			}
		}

		public static void UpdateOpacity(this Widget platformView, IView view)
		{
			platformView.Opacity = view.Opacity;
		}

		internal static void UpdateOpacity(this Widget platformView, double opacity)
		{
			platformView.Opacity = opacity;
		}

		public static void UpdateClip(this WrapperView platformView, IView view)
		{
			platformView.Clip = view.Clip;
		}

		[MissingMapper]
		public static void UpdateClip(this Widget platformView, IView view) { }

		[MissingMapper]
		public static void UpdateFlowDirection(this Widget platformView, IView view) { }

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

		internal static Matrix4x4 GetViewTransform(this Widget platformView)
		{
			if (platformView == null)
				return new Matrix4x4();

			var bounds = platformView.GetPlatformViewBounds();
			var scale = platformView.ScaleFactor;

			return new Matrix4x4()
			{
				// TODO: add scale
				Translation = new Vector3((float)bounds.X, (float)bounds.X, 0)
			};
		}

		internal static Widget? GetParent(this Widget? platformView)
		{
			return platformView?.Parent;
		}

		[MissingMapper]
		public static void UpdateShadow(this Widget? platformView, IView view) { }

		[MissingMapper]
		public static void UpdateBorder(this Widget? platformView, IView view) { }

		public static void Focus(this Widget? platformView, FocusRequest request)
		{
			if (platformView is not { })
			{
				request.TrySetResult(false);
				return;
			}

			platformView.GrabFocus();
			request.TrySetResult(platformView.HasFocus);
		}

		[MissingMapper]
		public static void Unfocus(this Widget? platformView, IView view) { }

		[MissingMapper]
		public static void UpdateInputTransparent(this Widget? platformView, IViewHandler handler, IView view) { }

		[MissingMapper]
		public static void UpdateToolTip(this Widget? platformView, ToolTip? tooltip) { }

		internal static IDisposable OnLoaded(this Widget? platformView, Action action)
		{
			if (platformView is not { })
				return new ActionDisposable(() => { });

			if (platformView.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			EventHandler? routedEventHandler = null;
			ActionDisposable? disposable = new ActionDisposable(() =>
			{
				if (routedEventHandler != null)
					platformView.Realized -= routedEventHandler;
			});

			routedEventHandler = (_, __) =>
			{
				disposable?.Dispose();
				disposable = null;
				action();
			};

			platformView.Realized += routedEventHandler;
			return disposable;
		}

		[MissingMapper]
		internal static IDisposable OnUnloaded(this Widget? platformView, Action action)
		{
			if (platformView is not { })
				return new ActionDisposable(() => { });

			if (!platformView.IsLoaded())
			{
				action();
				return new ActionDisposable(() => { });
			}

			EventHandler? routedEventHandler = null;
			ActionDisposable? disposable = new ActionDisposable(() =>
			{
				if (routedEventHandler != null)
					platformView.Unrealized -= routedEventHandler;
			});

			routedEventHandler = (_, __) =>
			{
				disposable?.Dispose();
				disposable = null;
				action();
			};

			platformView.Unrealized += routedEventHandler;
			return disposable;		}

		public static bool IsLoaded(this Widget? platformView)
		{
			if (platformView is not { })
				return false;

			return platformView.IsRealized;
		}

		[MissingMapper]
		public static bool HideSoftInput(this Widget platformView)
		{
			return false;
		}

		[MissingMapper]
		public static bool ShowSoftInput(this Widget platformView)
		{
			return false;
		}

		[MissingMapper]
		public static bool IsSoftInputShowing(this Widget platformView)
		{
			return false;
		}

		internal static T? GetChildAt<T>(this Widget platformView, int index) where T : Widget
		{
			if (platformView is Gtk.Container container && container.Children.Length < index)
			{
				return (T)container.Children[index];
			}

			if (platformView is Gtk.Bin bin && index == 0 && bin.Child is T child)
				return child;

			return default;
		}
	}
}