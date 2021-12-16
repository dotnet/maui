namespace Microsoft.Maui.Controls
{
	public partial class RadioButton : IRadioButton
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);

#if ANDROID
		object IRadioButton.Content => ContentAsString();
#endif

	}
}