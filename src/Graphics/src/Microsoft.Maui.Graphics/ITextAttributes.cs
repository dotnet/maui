namespace Microsoft.Maui.Graphics
{
	public interface ITextAttributes
	{
		string FontName { get; set; }

		float FontSize { get; set; }

		float Margin { get; set; }

		Color TextFontColor { get; set; }

		HorizontalAlignment HorizontalAlignment { get; set; }

		VerticalAlignment VerticalAlignment { get; set; }
	}
}
