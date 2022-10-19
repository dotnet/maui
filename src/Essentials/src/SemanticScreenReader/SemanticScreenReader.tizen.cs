namespace Microsoft.Maui.Accessibility
{
	partial class SemanticScreenReaderImplementation : ISemanticScreenReader
	{
		public void Announce(string text)
		{
			Tizen.NUI.Accessibility.Accessibility.Say(text, true);
		}
	}
}
