namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Label.xml" path="Type[@FullName='Microsoft.Maui.Controls.Label']/Docs" />
	public partial class Label : ILabel
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);
	}
}