namespace Microsoft.Maui.Controls.Shapes
{
	public sealed partial class Line : Shape
	{
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

		public static readonly BindableProperty X1Property =
			BindableProperty.Create(nameof(X1), typeof(double), typeof(Line), 0.0d);

		public static readonly BindableProperty Y1Property =
			BindableProperty.Create(nameof(Y1), typeof(double), typeof(Line), 0.0d);

		public static readonly BindableProperty X2Property =
			BindableProperty.Create(nameof(X2), typeof(double), typeof(Line), 0.0d);

		public static readonly BindableProperty Y2Property =
			BindableProperty.Create(nameof(Y2), typeof(double), typeof(Line), 0.0d);

		public double X1
		{
			set { SetValue(X1Property, value); }
			get { return (double)GetValue(X1Property); }
		}

		public double Y1
		{
			set { SetValue(Y1Property, value); }
			get { return (double)GetValue(Y1Property); }
		}

		public double X2
		{
			set { SetValue(X2Property, value); }
			get { return (double)GetValue(X2Property); }
		}

		public double Y2
		{
			set { SetValue(Y2Property, value); }
			get { return (double)GetValue(Y2Property); }
		}
	}
}