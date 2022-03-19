#nullable enable
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Accessibility;

namespace Microsoft.Maui.Accessibility
{
	public interface ISemanticScreenReader
	{
		void Announce(string text);
	}
}
namespace Microsoft.Maui.Essentials
{
	public static partial class SemanticScreenReader
	{
		public static void Announce(string text)
		{
			Current.Announce(text);
		}

		static ISemanticScreenReader? currentImplementation;

		public static ISemanticScreenReader Current =>
			currentImplementation ??= new SemanticScreenReaderImplementation();

		internal static void SetCurrent(ISemanticScreenReader? implementation) =>
			currentImplementation = implementation;
	}
}
