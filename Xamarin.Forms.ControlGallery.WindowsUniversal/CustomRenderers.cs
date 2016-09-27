using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Controls.Bugzilla42602.TextBoxView), typeof(Xamarin.Forms.ControlGallery.WindowsUniversal.TextBoxViewRenderer))]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal
{
	public class TextBoxViewRenderer : BoxViewRenderer
	{
		Canvas m_Canvas;

		protected override void OnElementChanged(ElementChangedEventArgs<BoxView> e)
		{
			base.OnElementChanged(e);

			ArrangeNativeChildren = true;

			if (m_Canvas != null)
				Children.Remove(m_Canvas);

			m_Canvas = new Canvas()
			{
				Width = 200,
				Height = 200,
				Background = new SolidColorBrush(Windows.UI.Color.FromArgb(0, 255, 255, 255)),
				IsHitTestVisible = false
			};

			Children.Add(m_Canvas);

			//ellipse
			Shape ellipse = new Ellipse()
			{
				Width = 100,
				Height = 100,
				Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)),

			};
			Canvas.SetLeft(ellipse, 0);
			Canvas.SetTop(ellipse, 0);
			m_Canvas.Children.Add(ellipse);

			//text
			TextBlock text = new TextBlock()
			{
				FontSize = 50,
				FontWeight = Windows.UI.Text.FontWeights.Normal,
				Text = "hello world",
				Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0))
			};
			Canvas.SetLeft(text, 0);
			Canvas.SetTop(text, 150);
			m_Canvas.Children.Add(text);
		}
	}
}
