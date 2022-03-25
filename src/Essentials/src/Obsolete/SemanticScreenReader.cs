#nullable enable
using Microsoft.Maui.Accessibility;

namespace Microsoft.Maui.Essentials
{
	public static partial class SemanticScreenReader
	{
		public static void Announce(string text)
		{
			Current.Announce(text);
		}

		static ISemanticScreenReader Current => Accessibility.SemanticScreenReader.Default;
	}
}
