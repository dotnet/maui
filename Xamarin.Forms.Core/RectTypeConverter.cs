namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(Rect))]
	public class RectTypeConverter : RectangleTypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			var rectangle = base.ConvertFromInvariantString(value);
			Rect rect = (Rectangle)rectangle;

			return rect;
		}
	}
}