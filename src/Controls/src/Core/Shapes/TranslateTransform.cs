#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A transform that translates (moves) an element by a specified offset.
	/// </summary>
	public class TranslateTransform : Transform
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TranslateTransform"/> class with no offset.
		/// </summary>
		public TranslateTransform()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TranslateTransform"/> class with the specified offsets.
		/// </summary>
		/// <param name="x">The horizontal translation offset.</param>
		/// <param name="y">The vertical translation offset.</param>
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

		/// <summary>
		/// Gets or sets the horizontal translation offset. This is a bindable property.
		/// </summary>
		public double X
		{
			set { SetValue(XProperty, value); }
			get { return (double)GetValue(XProperty); }
		}

		/// <summary>
		/// Gets or sets the vertical translation offset. This is a bindable property.
		/// </summary>
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