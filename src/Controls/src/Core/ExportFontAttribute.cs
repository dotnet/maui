#nullable disable
using System;
namespace Microsoft.Maui.Controls
{
	/// <summary>Registers a font file for use in the application.</summary>
	[Internals.Preserve(AllMembers = true)]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class ExportFontAttribute : Attribute
	{
		/// <summary>Gets or sets an optional alias used to reference the font.</summary>
		public string Alias { get; set; }

		/// <summary>Creates a new <see cref="ExportFontAttribute"/> for the specified font file.</summary>
		public ExportFontAttribute(string fontFileName)
		{
			FontFileName = fontFileName;
		}

		/// <summary>Gets the font file name.</summary>
		public string FontFileName { get; }
	}
}
