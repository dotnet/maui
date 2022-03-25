#nullable enable
using System.ComponentModel;
using System.Threading.Tasks;

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

		static ISemanticScreenReader? currentImplementation;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static ISemanticScreenReader Current =>
			currentImplementation ??= new SemanticScreenReaderImplementation();

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetCurrent(ISemanticScreenReader? implementation) =>
			currentImplementation = implementation;
	}
}
