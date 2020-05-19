using System;
namespace System.Maui
{
	[Internals.Preserve(AllMembers = true)]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class ExportFontAttribute : Attribute
	{
		public string Alias { get; set; }
		public ExportFontAttribute(string fontFileName)
		{
			FontFileName = fontFileName;
		}

		public string EmbeddedFontResourceId { get; set; }
		public string FontFileName { get; }
	}
}
