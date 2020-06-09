using System.Windows;
using System.Windows.Media;
using Xamarin.Forms.ControlGallery.WPF.Renderers;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.WPF;
using WRect = System.Windows.Rect;

[assembly: ExportRenderer(typeof(Issue6693Control), typeof(Issue6693ControlRenderer))]
namespace Xamarin.Forms.ControlGallery.WPF.Renderers
{
	public class Issue6693ControlRenderer:ViewRenderer<Issue6693Control,WIssue6693Control>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Issue6693Control> e)
		{
			base.OnElementChanged(e);

			SetNativeControl(new WIssue6693Control());


		}
	}

	public class WIssue6693Control : FrameworkElement
	{
		public WIssue6693Control()
		{
			
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			drawingContext.DrawRectangle(Brushes.LightGray, new Pen(Brushes.Black, 1), new WRect(0,0,ActualWidth, ActualHeight));
			var isEnabledText = IsEnabled ? "I'm enabled :)" : "I'm disabled :(";
			drawingContext.DrawText(new FormattedText(isEnabledText, 
				System.Globalization.CultureInfo.CurrentCulture, 
				System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, Brushes.Green), new System.Windows.Point(10,10));
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if(e.Property.Name == WIssue6693Control.IsEnabledProperty.Name)
			{
				InvalidateVisual();
			}
		}
	}
}
