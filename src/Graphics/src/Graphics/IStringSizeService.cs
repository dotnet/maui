namespace Microsoft.Maui.Graphics
{
	public interface IStringSizeService
	{
		SizeF GetStringSize(string value, IFont font, float fontSize);

		SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment);
	}
}
