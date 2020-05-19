using System;
namespace System.Maui
{
	public interface IEmbeddedFontLoader
	{
		(bool success, string filePath) LoadFont(EmbeddedFont font);
	}
}
