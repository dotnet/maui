#nullable disable
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Polygon']/Docs/*" />
	[ElementHandler<PolygonHandler>]
	public sealed partial class Polygon : Shape, IShape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public Polygon() : base()
		{
		}

		public Polygon(PointCollection points) : this()
		{
			Points = points;
		}

		/// <summary>Bindable property for <see cref="Points"/>.</summary>
		public static readonly BindableProperty PointsProperty =
			BindableProperty.Create(nameof(Points), typeof(PointCollection), typeof(Polygon), null, defaultValueCreator: bindable => new PointCollection());

		/// <summary>Bindable property for <see cref="FillRule"/>.</summary>
		public static readonly BindableProperty FillRuleProperty =
			BindableProperty.Create(nameof(FillRule), typeof(FillRule), typeof(Polygon), FillRule.EvenOdd);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="//Member[@MemberName='Points']/Docs/*" />
		public PointCollection Points
		{
			set { SetValue(PointsProperty, value); }
			get { return (PointCollection)GetValue(PointsProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Polygon.xml" path="//Member[@MemberName='FillRule']/Docs/*" />
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

				path.Close();
			}

			return path;
		}
	}
}
