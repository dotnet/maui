#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	public sealed partial class RoundRectangle : Shape
	{
		public RoundRectangle() : base()
		{
			Aspect = Stretch.Fill;
		}

		/// <summary>Bindable property for <see cref="CornerRadius"/>.</summary>
		public static readonly BindableProperty CornerRadiusProperty =
			BindableProperty.Create(nameof(CornerRadius), typeof(CornerRadius), typeof(RoundRectangle), new CornerRadius());

		public CornerRadius CornerRadius
		{
			set { SetValue(CornerRadiusProperty, value); }
			get { return (CornerRadius)GetValue(CornerRadiusProperty); }
		}
	}
}