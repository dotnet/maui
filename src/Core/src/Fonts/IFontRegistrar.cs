#nullable enable
using System.Reflection;

namespace Microsoft.Maui
{
	/// <summary>
	/// The <see cref="IFontRegistrar"/> keeps track of the fonts that are registered in our application.
	/// </summary>
	public interface IFontRegistrar
	{
		/// <summary>
		/// Registers a font in the app font cache.
		/// </summary>
		/// <param name="filename">The filename of the font to register.</param>
		/// <param name="alias">An optional alias with which you can also refer to this font.</param>
		/// <param name="assembly">The assembly (not used).</param>
		void Register(string filename, string? alias, Assembly assembly);

		/// <summary>
		/// Registers a font in the app font cache.
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="alias"></param>
		void Register(string filename, string? alias);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="font"></param>
		/// <returns></returns>
		// TODO: this should be async as it involves copying files
		string? GetFont(string font);
	}
}