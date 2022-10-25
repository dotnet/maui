namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Line']/Docs/*" />
	public sealed partial class Line : Shape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="//Member[@MemberName='X1Property']/Docs/*" />
		public static readonly BindableProperty X1Property =
			BindableProperty.Create(nameof(X1), typeof(double), typeof(Line), 0.0d);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="//Member[@MemberName='Y1Property']/Docs/*" />
		public static readonly BindableProperty Y1Property =
			BindableProperty.Create(nameof(Y1), typeof(double), typeof(Line), 0.0d);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="//Member[@MemberName='X2Property']/Docs/*" />
		public static readonly BindableProperty X2Property =
			BindableProperty.Create(nameof(X2), typeof(double), typeof(Line), 0.0d);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="//Member[@MemberName='Y2Property']/Docs/*" />
		public static readonly BindableProperty Y2Property =
			BindableProperty.Create(nameof(Y2), typeof(double), typeof(Line), 0.0d);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="//Member[@MemberName='X1']/Docs/*" />
		public double X1
		{
			set { SetValue(X1Property, value); }
			get { return (double)GetValue(X1Property); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="//Member[@MemberName='Y1']/Docs/*" />
		public double Y1
		{
			set { SetValue(Y1Property, value); }
			get { return (double)GetValue(Y1Property); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="//Member[@MemberName='X2']/Docs/*" />
		public double X2
		{
			set { SetValue(X2Property, value); }
			get { return (double)GetValue(X2Property); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="//Member[@MemberName='Y2']/Docs/*" />
		public double Y2
		{
			set { SetValue(Y2Property, value); }
			get { return (double)GetValue(Y2Property); }
		}
	}
}
