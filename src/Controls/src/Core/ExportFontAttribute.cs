#nullable disable
using System;
namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ExportFontAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.ExportFontAttribute']/Docs/*" />
	[Internals.Preserve(AllMembers = true)]
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class ExportFontAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ExportFontAttribute.xml" path="//Member[@MemberName='Alias']/Docs/*" />
		public string Alias { get; set; }

		/// <param name="fontFileName">The fontFileName parameter.</param>
		public ExportFontAttribute(string fontFileName)
		{
			FontFileName = fontFileName;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ExportFontAttribute.xml" path="//Member[@MemberName='FontFileName']/Docs/*" />
		public string FontFileName { get; }
	}
}
