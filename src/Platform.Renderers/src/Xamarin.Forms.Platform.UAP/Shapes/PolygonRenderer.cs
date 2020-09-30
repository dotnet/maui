using System.ComponentModel;
using Xamarin.Forms.Shapes;
using System.Collections.Specialized;

#if WINDOWS_UWP
using WFillRule = Windows.UI.Xaml.Media.FillRule;
using WPolygon = Windows.UI.Xaml.Shapes.Polygon;

namespace Xamarin.Forms.Platform.UWP
#else
using Xamarin.Forms.Platform.WPF.Extensions;
using WFillRule = System.Windows.Media.FillRule;
using WPolygon = System.Windows.Shapes.Polygon;

namespace Xamarin.Forms.Platform.WPF
#endif
{
	public class PolygonRenderer : ShapeRenderer<Polygon, WPolygon>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Polygon> args)
		{
			if (Control == null && args.NewElement != null)
			{
				SetNativeControl(new WPolygon());
			}

			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				var points = args.NewElement.Points;
				points.CollectionChanged += OnCollectionChanged;

				UpdatePoints();
				UpdateFillRule();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == Polygon.PointsProperty.PropertyName)
				UpdatePoints();
			else if (args.PropertyName == Polygon.FillRuleProperty.PropertyName)
				UpdateFillRule();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (Element != null)
				{
					var points = Element.Points;
					points.CollectionChanged -= OnCollectionChanged;
				}
			}
		}

		void UpdatePoints()
		{
			Control.Points = Element.Points.ToWindows();
		}

		void UpdateFillRule()
		{
			Control.FillRule = Element.FillRule == FillRule.EvenOdd ?
				WFillRule.EvenOdd :
				WFillRule.Nonzero;
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdatePoints();
		}
	}
}