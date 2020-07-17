using Windows.UI.Xaml.Input;
using Xamarin.Forms.ControlGallery.WindowsUniversal;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.UWP;
using WBrush = Windows.UI.Xaml.Media.Brush;
using WSolidColorBrush = Windows.UI.Xaml.Media.SolidColorBrush;

[assembly: ExportRenderer(typeof(Bugzilla57114._57114View), typeof(_57114Renderer))]

namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class _57114Renderer : VisualElementRenderer<Bugzilla57114._57114View, _57114NativeView>
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

		WBrush ColorToBrush(Color color)
		{
			return new WSolidColorBrush(Windows.UI.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255)));
		}
	}

	public class _57114NativeView : Windows.UI.Xaml.Controls.Grid
	{
		public _57114NativeView()
		{
			Tapped += OnTapped;
		}

		void OnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
		{
			MessagingCenter.Send(this as object, Bugzilla57114._57114NativeGestureFiredMessage);
		}
	}
}