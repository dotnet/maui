#nullable enable
using System.IO;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a font that is added as an embedded resource in the application.
	/// </summary>
	public class EmbeddedFont
	{
		/// <summary>
		/// The name of this font.
		/// </summary>
		public string? FontName { get; set; }

		/// <summary>
		/// A <see cref="Stream"/> with which the contents of this font can be accessed.
		/// </summary>
		public Stream? ResourceStream { get; set; }
	}
}
