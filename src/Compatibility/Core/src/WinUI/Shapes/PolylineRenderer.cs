using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.Specialized;

#if WINDOWS_UWP
using WFillRule = Microsoft.UI.Xaml.Media.FillRule;
using WPolyline = Microsoft.UI.Xaml.Shapes.Polyline;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
using Microsoft.Maui.Controls.Compatibility.Platform.WPF.Extensions;
using WFillRule = System.Windows.Media.FillRule;
using WPolyline = System.Windows.Shapes.Polyline;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
#endif
{
	public class PolylineRenderer : ShapeRenderer<Polyline, WPolyline>
	{
		PointCollection _points;

		protected override void OnElementChanged(ElementChangedEventArgs<Polyline> args)
		{
			if (Control == null && args.NewElement != null)
			{
				SetNativeControl(new WPolyline());
			}

			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				UpdatePoints();
				UpdateFillRule();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == Polyline.PointsProperty.PropertyName)
				UpdatePoints();
			else if (args.PropertyName == Polyline.FillRuleProperty.PropertyName)
				UpdateFillRule();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (_points != null)
				{
					_points.CollectionChanged -= OnCollectionChanged;
					_points = null;
				}
			}
		}

		void UpdatePoints()
		{
			if (_points != null)
				_points.CollectionChanged -= OnCollectionChanged;

			_points = Element.Points;

			_points.CollectionChanged += OnCollectionChanged;

			Control.Points = _points.ToWindows();
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