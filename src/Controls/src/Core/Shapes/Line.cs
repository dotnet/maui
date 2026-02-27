#nullable disable
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A shape that draws a straight line between two points.
	/// </summary>
	public sealed partial class Line : Shape, IShape
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Line"/> class.
		/// </summary>
		public Line() : base()
		{
		}

		public Line(double x1, double y1, double x2, double y2) : this()
		{
			X1 = x1;
			Y1 = y1;
			X2 = x2;
			Y2 = y2;
		}

		/// <summary>Bindable property for <see cref="X1"/>.</summary>
		public static readonly BindableProperty X1Property =
			BindableProperty.Create(nameof(X1), typeof(double), typeof(Line), 0.0d);

		/// <summary>Bindable property for <see cref="Y1"/>.</summary>
		public static readonly BindableProperty Y1Property =
			BindableProperty.Create(nameof(Y1), typeof(double), typeof(Line), 0.0d);

		/// <summary>Bindable property for <see cref="X2"/>.</summary>
		public static readonly BindableProperty X2Property =
			BindableProperty.Create(nameof(X2), typeof(double), typeof(Line), 0.0d);

		/// <summary>Bindable property for <see cref="Y2"/>.</summary>
		public static readonly BindableProperty Y2Property =
			BindableProperty.Create(nameof(Y2), typeof(double), typeof(Line), 0.0d);

		/// <summary>
		/// Gets or sets the x-coordinate of the line's start point. This is a bindable property.
		/// </summary>
		public double X1
		{
			set { SetValue(X1Property, value); }
			get { return (double)GetValue(X1Property); }
		}

		/// <summary>
		/// Gets or sets the y-coordinate of the line's start point. This is a bindable property.
		/// </summary>
		public double Y1
		{
			set { SetValue(Y1Property, value); }
			get { return (double)GetValue(Y1Property); }
		}

		/// <summary>
		/// Gets or sets the x-coordinate of the line's end point. This is a bindable property.
		/// </summary>
		public double X2
		{
			set { SetValue(X2Property, value); }
			get { return (double)GetValue(X2Property); }
		}

		/// <summary>
		/// Gets or sets the y-coordinate of the line's end point. This is a bindable property.
		/// </summary>
		public double Y2
		{
			set { SetValue(Y2Property, value); }
			get { return (double)GetValue(Y2Property); }
		}

		// TODO this should move to a remapped mapper
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == X1Property.PropertyName ||
				propertyName == Y1Property.PropertyName ||
				propertyName == X2Property.PropertyName ||
				propertyName == Y2Property.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		public override PathF GetPath()
		{
			var path = new PathF();

			path.MoveTo((float)X1, (float)Y1);
			path.LineTo((float)X2, (float)Y2);

			return path;
		}
	}
}
