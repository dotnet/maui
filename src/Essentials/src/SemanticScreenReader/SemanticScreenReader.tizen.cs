namespace Microsoft.Maui.Essentials.Implementations
{
	public partial class SemanticScreenReaderImplementation : ISemanticScreenReader
	{
		public void Announce(string text)
		{
			_ = ElmSharp.Accessible.AccessibleUtil.Say(text, true);
		}
	}
}
