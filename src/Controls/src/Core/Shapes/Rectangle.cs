#nullable disable
using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Handlers;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Rectangle.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Rectangle']/Docs/*" />
	[ElementHandler(typeof(RectangleHandler))]
	public sealed partial class Rectangle : Shape, IShape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Rectangle.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Rectangle() : base()
		{
			Aspect = Stretch.Fill;
		}

		/// <summary>Bindable property for <see cref="RadiusX"/>.</summary>
		public static readonly BindableProperty RadiusXProperty =
			BindableProperty.Create(nameof(RadiusX), typeof(double), typeof(Rectangle), 0.0d);

		/// <summary>Bindable property for <see cref="RadiusY"/>.</summary>
		public static readonly BindableProperty RadiusYProperty =
			BindableProperty.Create(nameof(RadiusY), typeof(double), typeof(Rectangle), 0.0d);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Rectangle.xml" path="//Member[@MemberName='RadiusX']/Docs/*" />
		public double RadiusX
		{
			set { SetValue(RadiusXProperty, value); }
			get { return (double)GetValue(RadiusXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Rectangle.xml" path="//Member[@MemberName='RadiusY']/Docs/*" />
		public double RadiusY
		{
			set { SetValue(RadiusYProperty, value); }
			get { return (double)GetValue(RadiusYProperty); }
		}

		// TODO this should move to a remapped mapper
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == RadiusXProperty.PropertyName ||
				propertyName == RadiusYProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		public override PathF GetPath()
		{
			var width = WidthForPathComputation;
			var height = HeightForPathComputation;

			var path = new PathF();

			float x = (float)StrokeThickness / 2;
			float y = (float)StrokeThickness / 2;
			float w = (float)(width - StrokeThickness);
			float h = (float)(height - StrokeThickness);
			float cornerRadius = (float)Math.Max(RadiusX, RadiusY);

			// TODO: Create specific Path taking into account RadiusX and RadiusY
			if (cornerRadius == 0)
			{
				// AppendRoundedRectangle will slash the corners even for cornerRadius = 0
				// so in that case we use AppendRectangle instead
				path.AppendRectangle(x, y, w, h);
			}
			else
			{
				path.AppendRoundedRectangle(x, y, w, h, cornerRadius);
			}

			return path;
		}
	}
}