#nullable disable
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolylineHandler : ShapeViewHandler
	{
		PointCollection _subscribedPoints;

		public static new IPropertyMapper<Polyline, IShapeViewHandler> Mapper = new PropertyMapper<Polyline, IShapeViewHandler>(ShapeViewHandler.Mapper)
		{
			[nameof(IShapeView.Shape)] = MapShape,
			[nameof(Polyline.Points)] = MapPoints,
			[nameof(Polyline.FillRule)] = MapFillRule,
		};

		public PolylineHandler() : base(Mapper)
		{

		}

		public PolylineHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
		{

		}

		void UpdatePointsSubscription(PointCollection points)
		{
			if (ReferenceEquals(_subscribedPoints, points))
			{
				return;
			}

			ClearPointsSubscription();

			if (points is null)
			{
				return;
			}

			_subscribedPoints = points;
			_subscribedPoints.CollectionChanged += OnPointsCollectionChanged;
		}

		void ClearPointsSubscription()
		{
			if (_subscribedPoints is null)
			{
				return;
			}

			_subscribedPoints.CollectionChanged -= OnPointsCollectionChanged;
			_subscribedPoints = null;
		}

		void OnPointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateValue(nameof(Polyline.Points));
		}
	}
}