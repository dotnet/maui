#nullable enable

namespace Microsoft.Maui.Accessibility
{
	public interface ISemanticScreenReader
	{
		void Announce(string text);
	}

	public static partial class SemanticScreenReader
	{
		public static void Announce(string text)
		{
			Current.Announce(text);
		}

		static ISemanticScreenReader Current => Accessibility.SemanticScreenReader.Default;

		static ISemanticScreenReader? defaultImplementation;

		public static ISemanticScreenReader Default =>
			defaultImplementation ??= new SemanticScreenReaderImplementation();

		internal static void SetDefault(ISemanticScreenReader? implementation) =>
			defaultImplementation = implementation;
	}
}
