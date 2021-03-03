using System;
namespace Microsoft.Maui.Controls
{
	public interface IEmbeddedFontLoader
	{
		(bool success, string filePath) LoadFont(EmbeddedFont font);
	}
}
