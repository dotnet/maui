using System.Reflection;

namespace Microsoft.Maui
{
	public interface IFontRegistrar
	{
		void Register(string filename, string alias, Assembly assembly);

		void Register(string filename, string? alias);

		//TODO: Investigate making this Async
		(bool hasFont, string? fontPath) HasFont(string font);
	}
}