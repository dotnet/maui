namespace Microsoft.Maui.Accessibility
{
	partial class SemanticScreenReaderImplementation : ISemanticScreenReader
	{
		public void Announce(string text)
		{
			_ = ElmSharp.Accessible.AccessibleUtil.Say(text, true);
		}
	}
}
