namespace Xamarin.Forms.Controls
{
	public class Issue3076Button : Button
	{
		public static readonly BindableProperty HorizontalContentAlignmentProperty =
			BindableProperty.Create("HorizontalContentAlignemnt", typeof(TextAlignment), typeof(Issue3076Button), TextAlignment.Center);

		public TextAlignment HorizontalContentAlignment
		{
			get { return (TextAlignment)GetValue(HorizontalContentAlignmentProperty); }
			set { SetValue(HorizontalContentAlignmentProperty, value); }
		}
	}
}