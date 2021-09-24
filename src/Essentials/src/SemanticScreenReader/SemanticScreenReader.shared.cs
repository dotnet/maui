using System.Threading.Tasks;

namespace Microsoft.Maui.Essentials
{
	public static partial class SemanticScreenReader
	{
		public static void Announce(string text)
		{
			PlatformAnnounce(text);
		}
	}
}
