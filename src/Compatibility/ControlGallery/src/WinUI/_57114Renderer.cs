using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

[assembly: ExportRenderer(typeof(Bugzilla57114._57114View), typeof(_57114Renderer))]

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	public class _57114Renderer : Handlers.Compatibility.VisualElementRenderer<Bugzilla57114._57114View, _57114NativeView>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Bugzilla57114._57114View> e)
		{
			if (e.NewElement != null && Control == null)
			{
				var view = new _57114NativeView();
				SetNativeControl(view);

			}

			base.OnElementChanged(e);

			if (Control != null)
			{
				Control.Background = ColorToBrush(Element.BackgroundColor);
			}
		}

		Microsoft.UI.Xaml.Media.Brush ColorToBrush(Color color)
		{
			return new Microsoft.UI.Xaml.Media.SolidColorBrush(global::Windows.UI.Color.FromArgb((byte)(color.Alpha * 255), (byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255)));
		}
	}

	public class _57114NativeView : Microsoft.UI.Xaml.Controls.Grid
	{
		public _57114NativeView()
		{
			Tapped += OnTapped;
		}

		void OnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Send(this as object, Bugzilla57114._57114NativeGestureFiredMessage);
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}