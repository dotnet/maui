#nullable disable
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A shape that draws a series of connected straight lines. Unlike <see cref="Polygon"/>, a polyline is not automatically closed.
	/// </summary>
	[ElementHandler<PolylineHandler>]
	public sealed partial class Polyline : Shape, IShape
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Polyline"/> class.
		/// </summary>
		public Polyline() : base()
		{
		}

		public Polyline(PointCollection points) : this()
		{
			Points = points;
		}

		/// <summary>Bindable property for <see cref="Points"/>.</summary>
		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(Polyline), null, defaultValueCreator: bindable => new PointCollection());

		/// <summary>Bindable property for <see cref="FillRule"/>.</summary>
		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(Polyline), FillRule.EvenOdd);

		/// <summary>
		/// Gets or sets the collection of <see cref="Point"/> values that define the vertices of the polyline. This is a bindable property.
		/// </summary>
		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}

		/// <summary>
		/// Gets or sets a value that specifies how the interior of the polyline is determined. This is a bindable property.
		/// </summary>
		public FillRule FillRule
		{
			set { SetValue(FillRuleProperty, value); }
			get { return (FillRule)GetValue(FillRuleProperty); }
		}

		// TODO this should move to a remapped mapper
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == PointsProperty.PropertyName ||
				propertyName == FillRuleProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		public override PathF GetPath()
		{
			var path = new PathF();

			if (Points?.Count > 0)
			{
				path.MoveTo((float)Points[0].X, (float)Points[0].Y);

				for (int index = 1; index < Points.Count; index++)
					path.LineTo((float)Points[index].X, (float)Points[index].Y);
			}

			return path;
		}
	}
}
