namespace Microsoft.Maui.Essentials
{
	public static partial class SemanticScreenReader
	{
		static void PlatformAnnounce(string text)
		{
			_ = ElmSharp.Accessible.AccessibleUtil.Say(text, true);
		}
	}
}
