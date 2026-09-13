#nullable disable
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls.Handlers
{
	public partial class PolygonHandler : ShapeViewHandler
	{
		PointCollection _subscribedPoints;

		public static new IPropertyMapper<Polygon, IShapeViewHandler> Mapper = new PropertyMapper<Polygon, IShapeViewHandler>(ShapeViewHandler.Mapper)
		{
			[nameof(IShapeView.Shape)] = MapShape,
			[nameof(Polygon.Points)] = MapPoints,
			[nameof(Polygon.FillRule)] = MapFillRule,
		};

		public PolygonHandler() : base(Mapper)
		{

		}

		public PolygonHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
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
			UpdateValue(nameof(Polygon.Points));
		}
	}
}