namespace Microsoft.Maui.Controls
{
	public partial class Label : ILabel
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);
	}
}