#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/TranslateTransform.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.TranslateTransform']/Docs/*" />
	public class TranslateTransform : Transform
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/TranslateTransform.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public TranslateTransform()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/TranslateTransform.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public TranslateTransform(double x, double y)
		{
			X = x;
			Y = y;
		}

		/// <summary>Bindable property for <see cref="X"/>.</summary>
		public static readonly BindableProperty XProperty =
			BindableProperty.Create(nameof(X), typeof(double), typeof(TranslateTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="Y"/>.</summary>
		public static readonly BindableProperty YProperty =
			BindableProperty.Create(nameof(Y), typeof(double), typeof(TranslateTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/TranslateTransform.xml" path="//Member[@MemberName='X']/Docs/*" />
		public double X
		{
			set { SetValue(XProperty, value); }
			get { return (double)GetValue(XProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/TranslateTransform.xml" path="//Member[@MemberName='Y']/Docs/*" />
		public double Y
		{
			set { SetValue(YProperty, value); }
			get { return (double)GetValue(YProperty); }
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as TranslateTransform).OnTransformPropertyChanged();
		}

		void OnTransformPropertyChanged()
		{
			Value = new Matrix(1, 0, 0, 1, X, Y);
		}
	}
}