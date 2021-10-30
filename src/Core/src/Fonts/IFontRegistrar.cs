#nullable enable
using System.Reflection;

namespace Microsoft.Maui
{
	public interface IFontRegistrar
	{
		void Register(string filename, string? alias, Assembly assembly);

		void Register(string filename, string? alias);

		// TODO: this should be async as it involves copying files
		string? GetFont(string font);
	}
}