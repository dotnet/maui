#nullable enable

namespace Microsoft.Maui.Accessibility
{
	/// <summary>
	/// The SemanticScreenReader API enables an application announce audible text to the user.
	/// </summary>
	public interface ISemanticScreenReader
	{
		/// <summary>
		/// Announces the specified text through the operating system's screen reader.
		/// </summary>
		/// <param name="text">The text to announce.</param>
		void Announce(string text);
	}

	/// <summary>
	/// The SemanticScreenReader API enables an application announce audible text to the user.
	/// </summary>
	public static partial class SemanticScreenReader
	{
		/// <summary>
		/// Announces the specified text through the operating system's screen reader.
		/// </summary>
		/// <param name="text">The text to announce.</param>
		public static void Announce(string text)
		{
			Current.Announce(text);
		}

		static ISemanticScreenReader Current => Accessibility.SemanticScreenReader.Default;

		static ISemanticScreenReader? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static ISemanticScreenReader Default =>
			defaultImplementation ??= new SemanticScreenReaderImplementation();

		internal static void SetDefault(ISemanticScreenReader? implementation) =>
			defaultImplementation = implementation;
	}
}
