namespace Microsoft.Maui.Graphics
{
	public interface ITextAttributes
	{
		IFont Font { get; set; }

		float FontSize { get; set; }

		float Margin { get; set; }

		Color TextFontColor { get; set; }

		HorizontalAlignment HorizontalAlignment { get; set; }

		VerticalAlignment VerticalAlignment { get; set; }
	}
}
