
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public sealed class Polyline : Shape, IShape
	{
		public Polyline() : base()
		{
		}

		public Polyline(PointCollection points) : this()
		{
			Points = points;
		}

		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(Polyline), null, defaultValueCreator: bindable => new PointCollection());

		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(Polyline), FillRule.EvenOdd);

		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}

		public FillRule FillRule
		{
			set { SetValue(FillRuleProperty, value); }
			get { return (FillRule)GetValue(FillRuleProperty); }
		}

		public PathF PathForBounds(Graphics.Rectangle rect)
		{
			var path = new PathF();

			if (Points?.Count > 0)
			{
				path.MoveTo((float)Points[0].X, (float)Points[0].Y);

				for (int index = 1; index < Points.Count; index++)
					path.LineTo((float)Points[index].X, (float)Points[index].Y);
			}

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}