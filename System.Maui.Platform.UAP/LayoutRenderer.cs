using System.ComponentModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.UWP
{
	public class LayoutRenderer : ViewRenderer<Layout, FrameworkElement>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement != null)
			{
				SizeChanged -= OnSizeChanged;
				SetAutomationId(null);
			}

			if (e.NewElement != null)
			{
				SizeChanged += OnSizeChanged;

				UpdateClipToBounds();

				if (!string.IsNullOrEmpty(Element.AutomationId))
				{
					SetAutomationId(Element.AutomationId);
				}
			}
		}

		protected override void UpdateBackgroundColor()
		{
			base.UpdateBackgroundColor();

			if (GetValue(BackgroundProperty) == null && Children.Count == 0)
			{
				// Forces the layout to take up actual space if it's otherwise empty
				Background = new SolidColorBrush(Colors.Transparent);
			}
		}

		protected override AutomationPeer OnCreateAutomationPeer()
		{
			// Since layouts in Forms can be interacted with, we need to create automation peers
			// for them so we can interact with them in automated tests
			return new FrameworkElementAutomationPeer(this);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Layout.IsClippedToBoundsProperty.PropertyName)
				UpdateClipToBounds();
		}

		void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdateClipToBounds();
		}

		void UpdateClipToBounds()
		{
			Clip = null;
			if (Element.IsClippedToBounds)
			{
				Clip = new RectangleGeometry { Rect = new Rect(0, 0, ActualWidth, ActualHeight) };
			}
		}
	}
}