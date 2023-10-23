using System;
using System.IO;
using Gtk;

namespace Microsoft.Maui.Handlers
{

	// https://docs.gtk.org/gtk3/class.Scale.html

	public partial class SliderHandler : ViewHandler<ISlider, Scale>
	{

		protected override Scale CreatePlatformView()
		{
			return new Scale(Orientation.Horizontal, 0, 1, .1);
		}

		protected override void ConnectHandler(Scale nativeView)
		{
			base.ConnectHandler(nativeView);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			nativeView.ValueChanged += OnNativeViewValueChanged;
		}

		protected override void DisconnectHandler(Scale nativeView)
		{
			base.DisconnectHandler(nativeView);

			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");

			nativeView.ValueChanged -= OnNativeViewValueChanged;

		}

		void OnNativeViewValueChanged(object? sender, EventArgs e)
		{
			if (sender is not Scale nativeView || VirtualView is not { } virtualView)
				return;

			virtualView.Value = nativeView.Value;

		}

		public static void MapMinimum(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateRange(slider);
		}

		public static void MapMaximum(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateRange(slider);
		}

		public static void MapValue(ISliderHandler handler, ISlider slider)
		{
			handler.PlatformView?.UpdateValue(slider);
		}

		[MissingMapper]
		public static void MapMinimumTrackColor(ISliderHandler handler, ISlider slider) { }

		[MissingMapper]
		public static void MapMaximumTrackColor(ISliderHandler handler, ISlider slider) { }

		public static void MapThumbColor(ISliderHandler handler, ISlider slider)
		{
			if (handler.PlatformView is not { } nativeView)
				return;

			nativeView.SetColor(slider.ThumbColor, "background-color", "contents > trough > slider");

		}

		static void SetImage(Widget w, string? image)
		{
			if (string.IsNullOrEmpty(image))
				return;

			w.SetStyleValue($"url('{image}')", "background-image", "contents > trough > slider");
			// nativeView.SetStyleImage("center","background-position", "contents > trough > slider");
			w.SetStyleValue("contain", "background-size", "contents > trough > slider");
		}

		public static void MapThumbImageSource(ISliderHandler handler, ISlider slider)
		{

			if (handler.PlatformView is not { } nativeView)
				return;

			var img = slider.ThumbImageSource;

			if (img == null)
				return;

			if (img is IFileImageSource fis && File.Exists(fis.File))
			{
				SetImage(nativeView, fis.File);

				return;
			}

			var provider = handler.GetRequiredService<IImageSourceServiceProvider>();

			img.UpdateImageSourceAsync(1, provider, p =>
				{

					// var css = p.CssImage(); //not working, so workaround is saving a tmp file:
					using var tmpfile = p?.TempFileFor();

					SetImage(nativeView, tmpfile?.Name);

				})
			   .FireAndForget(handler);
		}

	}

}